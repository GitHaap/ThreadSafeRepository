using System;
using ThreadSafe;
using Xunit;

namespace ThreadSafeRepository.Tests
{
	public class RepositoryTest
	{
		[Fact]
		public void �������n_���|�W�g��������()
		{
			var repos = new Repository<int>(1000, 100);

			Assert.Equal(1000, repos.CurrentState);
			Assert.Equal(100u, repos.HistoryBufferMaxLength);
			Assert.Equal(1u, repos.CurrentRevision);
		}
		[Fact]
		public void �������n_null�ŐV�K�쐬�����ꍇ()
		{
			var repos = new Repository<int?>(null, 100);

			Assert.Null(repos.CurrentState);
			Assert.Equal(100u, repos.HistoryBufferMaxLength);
			Assert.Equal(1u, repos.CurrentRevision);
		}

		[Fact]
		public void �Q�ƌn_�擾�����I�u�W�F�N�g�̎Q�Ƃ͕s��v���Ă���()
		{
			var repos = new Repository<int?>(1000, 100);

			var objA = repos.CurrentState;
			var objB = repos.CurrentState;

			Assert.False(object.ReferenceEquals(objA, objB));
		}

		[Fact]
		public void �C���n_�P��X���b�h����̃R�~�b�g()
		{
			var repos = new Repository<int>(1000);

			var modifier = repos.GetModifier();
			Assert.Equal(1000, modifier.WorkingState);
			Assert.Equal(1u, modifier.WorkingRevision);

			modifier.WorkingState = 2000;

			// ���|�W�g���͂��̎��_�ł͕s��
			Assert.Equal(1000, repos.CurrentState);
			Assert.Equal(1u, repos.CurrentRevision);

			modifier.Commit();

			// ���|�W�g���ɔ��f�����
			Assert.Equal(2000, repos.CurrentState);
			Assert.Equal(2u, repos.CurrentRevision);

			// ���f�B�t�@�C�A���o�[�W�����A�b�v�����
			Assert.Equal(2000, modifier.WorkingState);
			Assert.Equal(2u, modifier.WorkingRevision);
		}
		[Fact]
		public void �C���n_null���R�~�b�g()
		{
			var repos = new Repository<int?>(1000);

			var modifier = repos.GetModifier();
			modifier.WorkingState = null;
			modifier.Commit();

			// ���|�W�g���ɔ��f�����
			Assert.Null(repos.CurrentState);
			Assert.Equal(2u, repos.CurrentRevision);

			// ���f�B�t�@�C�A���o�[�W�����A�b�v�����
			Assert.Null(modifier.WorkingState);
			Assert.Equal(2u, modifier.WorkingRevision);
		}
		[Fact]
		public void �C���n_�q�X�g���[�r������̃R�~�b�g����Ƃ������X�V()
		{
			var repos = new Repository<int>(1000);

			var modifier = repos.GetModifier();
			modifier.WorkingState = 2000;
			modifier.Commit();

			// 1�߂�
			bool successUndo = repos.Undo();
			Assert.True(successUndo);
			Assert.Equal(1000, repos.CurrentState);
			Assert.Equal(1u, repos.CurrentRevision);

			// ���f�B�t�@�C�A��V�����擾
			var modifier2 = repos.GetModifier();
			Assert.Equal(1000, modifier2.WorkingState);
			Assert.Equal(1u, modifier2.WorkingRevision);

			// �C��
			modifier2.WorkingState = 3000;
			bool committed = modifier2.Commit();
			Assert.True(committed);

			// ���|�W�g���ɔ��f�����
			Assert.Equal(3000, repos.CurrentState);
			Assert.Equal(3u, repos.CurrentRevision); // ���r�W�����ԍ��͑����邾��

			// �i�߂Ȃ��i���ǉ������̂��ŐV���r�W�����ɂȂ����̂Łj
			bool failureRedo = repos.Redo();
			Assert.False(failureRedo);
		}
		[Fact]
		public void �C���n_���r�W�����Ⴂ�̏ꍇ�͏C���s�\()
		{
			var repos = new Repository<int>(1000);

			var modifier = repos.GetModifier();
			modifier.WorkingState = 2000;
			modifier.Commit();

			// 1�߂�
			bool successUndo = repos.Undo();
			Assert.True(successUndo);
			Assert.Equal(1000, repos.CurrentState);
			Assert.Equal(1u, repos.CurrentRevision);

			// ���f�B�t�@�C�A�͂����Ȃ�
			Assert.Equal(2000, modifier.WorkingState);
			Assert.Equal(2u, modifier.WorkingRevision);

			// ���r�W�����Ⴂ�̂��߃R�~�b�g�ł��Ȃ�
			modifier.WorkingState = 3000;
			bool committed = modifier.Commit();
			Assert.False(committed);
		}

		[Fact]
		public void ���r�W�����n_�߂�i��()
		{
			var repos = new Repository<int>(1000);

			var modifier = repos.GetModifier();
			modifier.WorkingState = 2000;
			modifier.Commit();

			// 1�߂�
			bool successUndo = repos.Undo();
			Assert.True(successUndo);
			Assert.Equal(1000, repos.CurrentState);
			Assert.Equal(1u, repos.CurrentRevision);

			// ���f�B�t�@�C�A�͂����Ȃ�
			Assert.Equal(2000, modifier.WorkingState);
			Assert.Equal(2u, modifier.WorkingRevision);

			// 1�i��
			bool successRedo = repos.Redo();
			Assert.True(successRedo);
			Assert.Equal(2000, repos.CurrentState);
			Assert.Equal(2u, repos.CurrentRevision);
		}
		[Fact]
		public void ���r�W�����n_history�o�b�t�@�[��()
		{
			var repos = new Repository<int>(1000, 0);

			var modifier = repos.GetModifier();
			modifier.WorkingState = 2000;
			modifier.Commit();

			// ���|�W�g���ɔ��f�����
			Assert.Equal(2000, repos.CurrentState);
			Assert.Equal(2u, repos.CurrentRevision);

			// �o�b�t�@���Ȃ��̂Ŗ߂�Ȃ�
			bool successUndo = repos.Undo();
			Assert.False(successUndo);
		}
	}
}

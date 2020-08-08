using System;
using ThreadSafe;
using Xunit;

namespace ThreadSafeRepository.Tests
{
	public class RepositoryTest
	{
		[Fact]
		public void ����n_���|�W�g��������()
		{
			var repos = new Repository<int>(1000, 100);

			Assert.Equal(1000, repos.CurrentState);
			Assert.Equal(100, repos.HistoryBufferMaxSize);
			Assert.Equal(1u, repos.CurrentRevision);
		}

		[Fact]
		public void ����n_�P��X���b�h����̃R�~�b�g()
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
		public void ����n_�߂�i��()
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
		public void �ُ�n_���r�W�����Ⴂ�̏ꍇ�͏C���s�\()
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
		public void ����n_�q�X�g���[�r������̃R�~�b�g����Ƃ������X�V()
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
			Assert.Equal(2u, repos.CurrentRevision); // ���r�W����2���㏑������� //TODO:��������Ȃ��ă��r�W�����ԍ��͑����邾���̂ق�����������

			// �i�߂Ȃ��i���ǉ������̂��ŐV���r�W�����ɂȂ����̂Łj
			bool failureRedo = repos.Redo();
			Assert.False(failureRedo);
		}
	}
}

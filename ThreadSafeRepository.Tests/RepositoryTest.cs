using System;
using ThreadSafe;
using Xunit;

namespace ThreadSafeRepository.Tests
{
	public class RepositoryTest
	{
		[Fact]
		public void ���|�W�g��������()
		{
			var repos = new Repository<int>(1000);

			Assert.Equal(1000, repos.CurrentState);
			Assert.Equal(1u, repos.CurrentRevision);
		}

		[Fact]
		public void �P��X���b�h����̃R�~�b�g()
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
	}
}

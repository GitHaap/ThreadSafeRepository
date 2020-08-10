using System;
using ThreadSafe;
using Xunit;

namespace ThreadSafeRepository.Tests
{
	public class RepositoryTest
	{
		[Fact]
		public void 初期化系_リポジトリ初期化()
		{
			var repos = new Repository<int>(1000, 100);

			Assert.Equal(1000, repos.CurrentState);
			Assert.Equal(100, repos.HistoryBufferMaxSize);
			Assert.Equal(1u, repos.CurrentRevision);
		}
		[Fact]
		public void 初期化系_nullで新規作成した場合()
		{
			var repos = new Repository<int?>(null, 100);

			Assert.Null(repos.CurrentState);
			Assert.Equal(100, repos.HistoryBufferMaxSize);
			Assert.Equal(1u, repos.CurrentRevision);
		}

		[Fact]
		public void 参照系_取得したオブジェクトの参照は不一致している()
		{
			var repos = new Repository<int?>(1000, 100);

			var objA = repos.CurrentState;
			var objB = repos.CurrentState;

			Assert.False(object.ReferenceEquals(objA, objB));
		}

		[Fact]
		public void 修正系_単一スレッドからのコミット()
		{
			var repos = new Repository<int>(1000);
			
			var modifier = repos.GetModifier();
			Assert.Equal(1000, modifier.WorkingState);
			Assert.Equal(1u, modifier.WorkingRevision);

			modifier.WorkingState = 2000;

			// リポジトリはこの時点では不変
			Assert.Equal(1000, repos.CurrentState);
			Assert.Equal(1u, repos.CurrentRevision);

			modifier.Commit();

			// リポジトリに反映される
			Assert.Equal(2000, repos.CurrentState);
			Assert.Equal(2u, repos.CurrentRevision);

			// モディファイアもバージョンアップされる
			Assert.Equal(2000, modifier.WorkingState);
			Assert.Equal(2u, modifier.WorkingRevision);
		}
		[Fact]
		public void 修正系_nullをコミット()
		{
			var repos = new Repository<int?>(1000);

			var modifier = repos.GetModifier();
			modifier.WorkingState = null;
			modifier.Commit();

			// リポジトリに反映される
			Assert.Null(repos.CurrentState);
			Assert.Equal(2u, repos.CurrentRevision);

			// モディファイアもバージョンアップされる
			Assert.Null(modifier.WorkingState);
			Assert.Equal(2u, modifier.WorkingRevision);
		}
		[Fact]
		public void 修正系_ヒストリー途中からのコミットするとそこが更新()
		{
			var repos = new Repository<int>(1000);

			var modifier = repos.GetModifier();
			modifier.WorkingState = 2000;
			modifier.Commit();

			// 1つ戻る
			bool successUndo = repos.Undo();
			Assert.True(successUndo);
			Assert.Equal(1000, repos.CurrentState);
			Assert.Equal(1u, repos.CurrentRevision);

			// モディファイアを新しく取得
			var modifier2 = repos.GetModifier();
			Assert.Equal(1000, modifier2.WorkingState);
			Assert.Equal(1u, modifier2.WorkingRevision);

			// 修正
			modifier2.WorkingState = 3000;
			bool committed = modifier2.Commit();
			Assert.True(committed);

			// リポジトリに反映される
			Assert.Equal(3000, repos.CurrentState);
			Assert.Equal(3u, repos.CurrentRevision); // リビジョン番号は増えるだけ

			// 進めない（今追加したのが最新リビジョンになったので）
			bool failureRedo = repos.Redo();
			Assert.False(failureRedo);
		}
		[Fact]
		public void 修正系_リビジョン違いの場合は修正不可能()
		{
			var repos = new Repository<int>(1000);

			var modifier = repos.GetModifier();
			modifier.WorkingState = 2000;
			modifier.Commit();

			// 1つ戻る
			bool successUndo = repos.Undo();
			Assert.True(successUndo);
			Assert.Equal(1000, repos.CurrentState);
			Assert.Equal(1u, repos.CurrentRevision);

			// モディファイアはかわらない
			Assert.Equal(2000, modifier.WorkingState);
			Assert.Equal(2u, modifier.WorkingRevision);

			// リビジョン違いのためコミットできない
			modifier.WorkingState = 3000;
			bool committed = modifier.Commit();
			Assert.False(committed);
		}

		[Fact]
		public void リビジョン系_戻る進む()
		{
			var repos = new Repository<int>(1000);

			var modifier = repos.GetModifier();
			modifier.WorkingState = 2000;
			modifier.Commit();

			// 1つ戻る
			bool successUndo = repos.Undo();
			Assert.True(successUndo);
			Assert.Equal(1000, repos.CurrentState);
			Assert.Equal(1u, repos.CurrentRevision);

			// モディファイアはかわらない
			Assert.Equal(2000, modifier.WorkingState);
			Assert.Equal(2u, modifier.WorkingRevision);

			// 1つ進む
			bool successRedo = repos.Redo();
			Assert.True(successRedo);
			Assert.Equal(2000, repos.CurrentState);
			Assert.Equal(2u, repos.CurrentRevision);
		}
	}
}

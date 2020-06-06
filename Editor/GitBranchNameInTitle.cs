using System.IO;
using System.Reflection;
using UniEditorWindowTitleChanger;
using UniGitUtils;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Kogane.Internal
{
	/// <summary>
	/// Unity エディタのタイトルに Git のブランチ名を表示するエディタ拡張
	/// </summary>
	[InitializeOnLoad]
	internal static class GitBranchNameInTitle
	{
		//==============================================================================
		// 関数(static)
		//==============================================================================
		/// <summary>
		/// コンストラクタ
		/// </summary>
		static GitBranchNameInTitle()
		{
			// https://github.com/Unity-Technologies/UnityCsReference/blob/master/Editor/Mono/EditorApplication.cs
			var activeSceneName = L10n.Tr( "Untitled" );

			if ( !string.IsNullOrEmpty( SceneManager.GetActiveScene().path ) )
			{
				activeSceneName = Path.GetFileNameWithoutExtension( SceneManager.GetActiveScene().path );
			}

			//var projectName = PlayerSettings.productName;
			var projectName = Path.GetFileName( Path.GetDirectoryName( Application.dataPath ) );
			var targetName  = GetBuildTargetGroupDisplayName( BuildPipeline.GetBuildTargetGroup( EditorUserBuildSettings.activeBuildTarget ) );
			var version     = Application.unityVersion;
			var licenseType = InternalEditorUtility.HasPro() ? "Pro" : "Personal";
			var branchName  = GitUtils.LoadBranchName();
			var title       = $"{projectName} - {activeSceneName} - {targetName} - Unity {version} {licenseType}";

			if ( !string.IsNullOrWhiteSpace( branchName ) )
			{
				title += $" - {branchName}";
			}

			EditorWindowTitleChanger.SetTitle( title );
		}

		/// <summary>
		/// ウィンドウのタイトルや Build Settings の Platform の欄に表示されるプラットフォーム名を返します
		/// </summary>
		private static string GetBuildTargetGroupDisplayName( BuildTargetGroup buildTargetGroup )
		{
			const string       name = "GetBuildTargetGroupDisplayName";
			const BindingFlags attr = BindingFlags.Static | BindingFlags.NonPublic;

			var parameters  = new object[] { buildTargetGroup };
			var type        = typeof( BuildPipeline );
			var methodInfo  = type.GetMethod( name, attr );
			var displayName = ( string ) methodInfo.Invoke( null, parameters );

			return displayName;
		}
	}
}
using System;
using System.IO;
using System.Linq;
using System.Reflection;
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
		// 定数
		//==============================================================================
		private const BindingFlags UPDATE_MAIN_WINDOW_TITLE_ATTR          = BindingFlags.Static | BindingFlags.NonPublic;
		private const string       APPLICATION_TITLE_DESCRIPTOR_FULL_NAME = "UnityEditor.ApplicationTitleDescriptor";

		//==============================================================================
		// 定数(static readonly)
		//==============================================================================
		private static readonly Type     EDITOR_APPLICATION_TYPE = typeof( EditorApplication );
		private static readonly Assembly EDITOR_ASSEMBLY         = EDITOR_APPLICATION_TYPE.Assembly;
		private static readonly Type[]   EDITOR_TYPES            = EDITOR_ASSEMBLY.GetTypes();

		private static readonly Type APPLICATION_TITLE_DESCRIPTOR_TYPE = EDITOR_TYPES
			.FirstOrDefault( c => c.FullName == APPLICATION_TITLE_DESCRIPTOR_FULL_NAME );

		private static readonly EventInfo UPDATE_MAIN_WINDOW_TITLE_EVENT_INFO =
			EDITOR_APPLICATION_TYPE.GetEvent( "updateMainWindowTitle", UPDATE_MAIN_WINDOW_TITLE_ATTR );

		private static readonly MethodInfo UPDATE_MAIN_WINDOW_TITLE_METHOD_INFO =
			EDITOR_APPLICATION_TYPE.GetMethod( "UpdateMainWindowTitle", UPDATE_MAIN_WINDOW_TITLE_ATTR );

		//==============================================================================
		// 関数(static)
		//==============================================================================
		/// <summary>
		/// コンストラクタ
		/// </summary>
		static GitBranchNameInTitle()
		{
			var delegateType = typeof( Action<> ).MakeGenericType( APPLICATION_TITLE_DESCRIPTOR_TYPE );
			var methodInfo   = ( ( Action<object> ) UpdateMainWindowTitle ).Method;
			var del          = Delegate.CreateDelegate( delegateType, null, methodInfo );
			var nonPublic    = true;
			var parameters   = new object[] { del };

			UPDATE_MAIN_WINDOW_TITLE_EVENT_INFO.GetAddMethod( nonPublic ).Invoke( null, parameters );
		}

		/// <summary>
		/// タイトルの表示を更新します
		/// </summary>
		/// <param name="desc"></param>
		private static void UpdateMainWindowTitle( object desc )
		{
			var fieldInfo = APPLICATION_TITLE_DESCRIPTOR_TYPE
				.GetField( "title", BindingFlags.Instance | BindingFlags.Public );

			var title = GetTitle();
			fieldInfo.SetValue( desc, title );
		}

		/// <summary>
		/// タイトルに表示する文字列を返します
		/// </summary>
		private static string GetTitle()
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

			return title;
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
using System.Collections.Generic;
using System.IO;
using SystemEx;
using UE4Assistant.Templates;
using UE4Assistant.Templates.Config;
using UE4Assistant.Templates.Generators;
using UE4Assistant.Templates.Source;
using UE4Assistant.Templates.Source.GameMode;
using UE4Assistant.Templates.Source.GameState;
using UE4Assistant.Templates.Source.PlayerState;
using static SystemEx.Template;

namespace UE4Assistant
{
	public enum TypePrefix
	{
		A,
		U,
		F,
	}

	public static class Template
	{
		public static TypePrefix GetTypePrefix(this string typeName)
		{
			switch (typeName[0])
			{
				case 'A': return TypePrefix.A;
				case 'U': return TypePrefix.U;
				default: return TypePrefix.F;
			}
		}

		public static void CreateModule(string RootPath, string ModuleName)
		{
			string sourcePath = Path.Combine(RootPath, "Source");
			string modulePath = Path.Combine(sourcePath, ModuleName);
			string privatePath = Path.Combine(modulePath, "Private");
			string publicPath = Path.Combine(modulePath, "Public");

			Directory.CreateDirectory(sourcePath);
			Directory.CreateDirectory(modulePath);
			Directory.CreateDirectory(privatePath);
			Directory.CreateDirectory(publicPath);

			var parameters = new {
				modulename = ModuleName,
				isprimary = false
			}.ToExpando();

			File.WriteAllText(Path.Combine(modulePath, ModuleName + ".Build.cs")
				, TransformToText<ModuleBuild_cs>(parameters));
			File.WriteAllText(Path.Combine(privatePath, ModuleName + "PrivatePCH.h")
				, TransformToText<PrivatePCH_h>(parameters));
			File.WriteAllText(Path.Combine(privatePath, ModuleName + ".cpp")
				, TransformToText<Module_cpp>(parameters));
			File.WriteAllText(Path.Combine(publicPath, ModuleName + ".h")
				, TransformToText<Module_h>(parameters));
			File.WriteAllText(Path.Combine(publicPath, ModuleName + ".final.h"), "");
		}

		public static UProject CreateProject(string ProjectName)
		{
			UProject project = new UProject();

			string configPath = Path.Combine("Config");
			string contentPath = Path.Combine("Content");
			string sourcePath = Path.Combine("Source/Game");

			Directory.CreateDirectory(configPath);
			Directory.CreateDirectory(contentPath);
			Directory.CreateDirectory(sourcePath);

			{
				UModule module = new UModule();
				module.Name = ProjectName;
				CreateModule(project.RootPath, module.Name);
				project.Modules.Add(module);

				var parameters = new Dictionary<string, object>
				{
					{ "targetname", module.Name },
					{ "targettype", "Game" },
					{ "extramodulenames", new List<string> { module.Name } },
				};

				File.WriteAllText(Path.Combine(sourcePath, ProjectName + ".Target.cs")
					, TransformToText<ProjectTarget_cs>(parameters));

				{
					parameters.Add("modulename", module.Name);
					parameters.Add("isprimary", true);

					string modulePath = Path.Combine(sourcePath, module.Name);
					string gamemodePath = Path.Combine(modulePath, "GameMode");
					string gamestatePath = Path.Combine(modulePath, "GameState");
					string playerstatePath = Path.Combine(modulePath, "PlayerState");

					Directory.CreateDirectory(modulePath);
					Directory.CreateDirectory(gamemodePath);
					Directory.CreateDirectory(gamestatePath);
					Directory.CreateDirectory(playerstatePath);

					File.WriteAllText(Path.Combine(gamemodePath, module.Name + "GameMode.h")
						, TransformToText<GameMode_h>(parameters));
					File.WriteAllText(Path.Combine(gamemodePath, module.Name + "GameMode.cpp")
						, TransformToText<GameMode_cpp>(parameters));

					File.WriteAllText(Path.Combine(gamestatePath, module.Name + "GameState.h")
						, TransformToText<GameState_h>(parameters));
					File.WriteAllText(Path.Combine(gamestatePath, module.Name + "GameState.cpp")
						, TransformToText<GameState_cpp>(parameters));

					File.WriteAllText(Path.Combine(playerstatePath, module.Name + "PlayerState.h")
						, TransformToText<PlayerState_h>(parameters));
					File.WriteAllText(Path.Combine(playerstatePath, module.Name + "PlayerState.cpp")
						, TransformToText<PlayerState_cpp>(parameters));

					File.WriteAllText(Path.Combine(modulePath, module.Name + "GameInstance.h")
						, TransformToText<GameInstance_h>(parameters));
					File.WriteAllText(Path.Combine(modulePath, module.Name + "GameInstance.cpp")
						, TransformToText<GameInstance_cpp>(parameters));

					File.WriteAllText(Path.Combine(modulePath, module.Name + "Statics.h")
						, TransformToText<Statics_h>(parameters));
					File.WriteAllText(Path.Combine(modulePath, module.Name + "Statics.cpp")
						, TransformToText<Statics_cpp>(parameters));
				}
			}

			{
				UModule module = new UModule();
				module.Name = ProjectName + "Editor";
				module.Type = UModuleType.Editor;
				CreateModule(project.RootPath, module.Name);
				project.Modules.Add(module);

				Dictionary<string, object> parameters = new Dictionary<string, object>
				{
					{ "targetname", module.Name },
					{ "targettype", "Editor" },
					{ "extramodulenames", new List<string> { ProjectName } }
				};

				File.WriteAllText(Path.Combine(sourcePath, ProjectName + "Editor.Target.cs")
					, TransformToText<ProjectTarget_cs>(parameters));
			}

			{
				Dictionary<string, object> parameters = new Dictionary<string, object>
				{
					{ "modulename", ProjectName }
				};

				File.WriteAllText(Path.Combine(configPath, "DefaultEditor.ini")
					, TransformToText<DefaultEditor_ini>(parameters));
				File.WriteAllText(Path.Combine(configPath, "DefaultEditorSettings.ini")
					, TransformToText<DefaultEditorSettings_ini>(parameters));
				File.WriteAllText(Path.Combine(configPath, "DefaultEngine.ini")
					, TransformToText<DefaultEngine_ini>(parameters));
				File.WriteAllText(Path.Combine(configPath, "DefaultGame.ini")
					, TransformToText<DefaultGame_ini>(parameters));
				File.WriteAllText(Path.Combine(configPath, "DefaultGameUserSettings.ini")
					, TransformToText<DefaultGameUserSettings_ini>(parameters));
				File.WriteAllText(Path.Combine(configPath, "DefaultInput.ini")
					, TransformToText<DefaultInput_ini>(parameters));
			}

			project.Save(ProjectName + ".uproject");

			return project;
		}

		public static string CreateSourceFile(string content, string mainHeader
			, string pchHeader = null, string finalHeader = null, string locTextNamespaceName = null)
		{
			return TransformToText<SourceFile>(new {
				mainHeader,
				hasPCHHeader = !string.IsNullOrWhiteSpace(pchHeader),
				pchHeader,
				hasFinalHeader = !string.IsNullOrWhiteSpace(finalHeader),
				finalHeader,
				hasLocTextNamespace = !string.IsNullOrWhiteSpace(locTextNamespaceName),
				locTextNamespaceName,
				content
			}.ToExpando());
		}

		public static string CreateHeaderFile(string content, string generatedHeader = null)
		{
			return TransformToText<HeaderFile>(new
			{
				hasGeneratedHeader = !string.IsNullOrWhiteSpace(generatedHeader),
				generatedHeader,
				content
			}.ToExpando());
		}

		public static string CreateClass_cpp(string moduleName, TypePrefix typePrefix, string typeName, string baseName, bool hasConstructor)
		{
			return TransformToText<Class_cpp>(new
			{
				moduleName,
				typePrefix = typePrefix.ToString(),
				typeName,
				baseName,
				hasConstructor
			}.ToExpando());
		}

		public static string CreateClass_h(string moduleName, TypePrefix typePrefix, string typeName, string baseName, bool hasConstructor)
		{
			return TransformToText<Class_h>(new
			{
				moduleName,
				typePrefix = typePrefix.ToString(),
				typeName,
				baseName,
				hasConstructor
			}.ToExpando());
		}

		public static string CreateSimpleClass_cpp(string moduleName, TypePrefix typePrefix, string typeName, string baseName)
		{
			return TransformToText<SimpleClass_cpp>(new
			{
				moduleName,
				typePrefix = typePrefix.ToString(),
				typeName,
				baseName
			}.ToExpando());
		}

		public static string CreateSimpleClass_h(string moduleName, TypePrefix typePrefix, string typeName, string baseName)
		{
			return TransformToText<SimpleClass_h>(new
			{
				moduleName,
				typePrefix = typePrefix.ToString(),
				typeName,
				baseName
			}.ToExpando());
		}

		public static string CreateInterface_h(string moduleName, string typeName)
		{
			return TransformToText<Interface_h>(new
			{
				moduleName,
				typeName,
			}.ToExpando());
		}

		public static string CreateStruct_h(string typeName)
		{
			return TransformToText<Struct_h>(new
			{
				typeName,
			}.ToExpando());
		}

		public static string CreateEnum_h(string typeName)
		{
			return TransformToText<Enum_h>(new
			{
				typeName,
			}.ToExpando());
		}
	}
}

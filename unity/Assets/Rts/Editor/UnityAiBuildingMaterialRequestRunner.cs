using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace ProjectAegisRTS.UnityClient.EditorTools
{
    [InitializeOnLoad]
    public static class UnityAiBuildingMaterialRequestRunner
    {
        const string RequestFileName = "unity_ai_generate_building_materials.request";
        const string DoneFileName = "unity_ai_generate_building_materials.done";
        const string FailedFileName = "unity_ai_generate_building_materials.failed";
        const string SourceTextureFolder = "Assets/Rts/Art/UnityAIBuildingSlate/SourceTextures";
        const string GeneratedMaterialFolder = "Assets/Rts/Art/UnityAIBuildingSlate/AiGeneratedMaterials";
        const string MaterialModelId = "hand-painted-textures-2-0";
        const string NegativePrompt = "No logos, no letters, no UI, no text, no faction symbols, no copyrighted IP, no recognizable Command and Conquer or Red Alert insignia, no vehicles, no characters, no full building image, no flat card, no unreadable noisy collage.";
        static readonly MaterialRequest[] MaterialRequests =
        {
            new MaterialRequest("worn_green_gray_metal", "Generate a seamless PBR material for a classic military RTS building exterior: worn green-gray painted steel panels, chipped edges, subtle rivets, grime in panel seams, sun-faded paint, small rust scratches, top-down readable, game-ready, tileable, realistic but stylized."),
            new MaterialRequest("dark_oiled_metal", "Generate a seamless PBR material for dark oiled industrial metal used inside vehicle bays: blackened steel, oily smears, worn highlights on raised ribs, soot in seams, subtle mechanical grime, tileable, game-ready RTS material."),
            new MaterialRequest("scraped_edge_metal", "Generate a seamless PBR material for scraped exposed steel armor edges: gray steel, worn bevel highlights, chipped paint remnants, tiny scratches, muted yellow-gray edge wear, tileable, game-ready."),
            new MaterialRequest("gunmetal_mechanics", "Generate a seamless PBR material for gunmetal mechanical parts: dark blue-gray steel, machined wear, circular scuffs, bolts, oil stains, subtle roughness variation, tileable, game-ready RTS asset material."),
            new MaterialRequest("ribbed_bay_door", "Generate a seamless PBR material for a ribbed roll-up industrial bay door: dark steel slats, vertical grime streaks, worn lower edge, scraped paint, recessed grooves, readable from overhead RTS camera, tileable."),
            new MaterialRequest("small_service_panels", "Generate a seamless PBR material for small military service panels: muted green-gray panels, tiny bolts, access hatches, warning labels implied as shapes only with no text, chipped corners, grime, tileable."),
            new MaterialRequest("weathered_concrete", "Generate a seamless PBR material for weathered military concrete: gray concrete slab, hairline cracks, darker stains, chipped aggregate, dust, subtle square panel seams, tileable, game-ready."),
            new MaterialRequest("foundation_concrete", "Generate a seamless PBR material for heavy base foundation concrete: reinforced slab, worn edges, embedded aggregate, oil stains, faint construction seams, muted battlefield color, tileable."),
            new MaterialRequest("dark_worn_concrete", "Generate a seamless PBR material for dark worn concrete interior floors: black-gray concrete, oil stains, dust, scuffed vehicle tracks, shallow cracks, tileable, game-ready."),
            new MaterialRequest("worn_asphalt_ramp", "Generate a seamless PBR material for worn asphalt and vehicle ramp surfaces: dark asphalt, tire scuffs, aggregate speckles, cracks, dusty edge wear, tileable, top-down RTS readable."),
            new MaterialRequest("classic_red_command_accent", "Generate a seamless PBR material for classic red military command accent paint on metal panels: deep red painted steel, chipped edges, soot stains, faded sun-worn paint, subtle rivets, no symbols or text, tileable."),
            new MaterialRequest("defense_red_brown_accent", "Generate a seamless PBR material for red-brown defensive armor paint: dark red oxide painted steel, scratches, impact chips, soot, exposed metal on edges, no symbols or text, tileable.")
        };
        static bool running;

        static UnityAiBuildingMaterialRequestRunner()
        {
            EditorApplication.delayCall += TryRunRequestedGeneration;
        }

        static async void TryRunRequestedGeneration()
        {
            if (running)
                return;

            if (EditorApplication.isCompiling || EditorApplication.isUpdating)
            {
                EditorApplication.delayCall += TryRunRequestedGeneration;
                return;
            }

            var requestPath = GetRequestPath(RequestFileName);
            if (!File.Exists(requestPath))
                return;

            running = true;
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(requestPath));
                File.Delete(GetRequestPath(DoneFileName));
                File.Delete(GetRequestPath(FailedFileName));
                EnsureFolderRecursive(SourceTextureFolder);
                EnsureFolderRecursive(GeneratedMaterialFolder);

                var generated = MaterialRequests
                    .Where(request => !HasPromotedAlbedo(request.Id))
                    .ToArray();
                var generatedPaths = generated
                    .Select(request => request.Id + ": pending")
                    .ToList();

                for (var i = 0; i < generated.Length; i++)
                {
                    var request = generated[i];
                    var materialPath = await GenerateMaterialAsync(request.Id, request.Prompt + " " + NegativePrompt);
                    PromoteMaterialTextures(request.Id, materialPath);
                    generatedPaths[i] = request.Id + ": " + materialPath;
                }

                UnityAiBuildingSlateGenerator.BuildAndCaptureBatch();

                File.WriteAllText(
                    GetRequestPath(DoneFileName),
                    DateTime.Now.ToString("O") + Environment.NewLine +
                    "Model: " + MaterialModelId + Environment.NewLine +
                    "Generated count: " + generated.Length + Environment.NewLine +
                    string.Join(Environment.NewLine, generatedPaths));
                File.Delete(requestPath);
                Debug.Log("Unity AI building material request runner completed generation.");
            }
            catch (Exception ex)
            {
                File.WriteAllText(GetRequestPath(FailedFileName), ex.ToString());
                Debug.LogException(ex);
            }
            finally
            {
                running = false;
            }
        }

        static async Task<string> GenerateMaterialAsync(string materialId, string prompt)
        {
            var generateToolType = FindType("Unity.AI.Assistant.Editor.Backend.Socket.Tools.GenerateAssetTool");
            var commandType = FindType("Unity.AI.Generators.Tools.GenerationCommands");
            var contextFactoryType = FindType("Unity.AI.Assistant.FunctionCalling.ToolExecutionContextFactory");

            var createContext = contextFactoryType
                .GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                .First(m => m.Name == "CreateForExternalCall" && m.GetParameters().Length == 2);
            var context = createContext.Invoke(null, new object[] { "Unity.AssetGeneration.GenerateAsset", null });
            var command = Enum.Parse(commandType, "GenerateMaterial");
            var savePath = GeneratedMaterialFolder + "/" + materialId + "_ai.mat";

            var generate = generateToolType
                .GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                .First(m => m.Name == "GenerateAsset" && m.GetParameters().Length == 16);

            var task = (Task)generate.Invoke(null, new object[]
            {
                context,
                command,
                MaterialModelId,
                prompt,
                savePath,
                true,
                null,
                -1L,
                null,
                null,
                0f,
                false,
                0,
                0,
                null,
                true
            });

            await task;

            var result = task.GetType().GetProperty("Result")?.GetValue(task);
            var assetPath = result?.GetType().GetField("AssetPath")?.GetValue(result) as string;
            return string.IsNullOrEmpty(assetPath) ? savePath : assetPath;
        }

        static bool HasPromotedAlbedo(string materialId)
        {
            return Directory.GetFiles(Path.Combine(Directory.GetParent(Application.dataPath).FullName, SourceTextureFolder), materialId + "_ai_albedo.*").Length > 0;
        }

        static void PromoteMaterialTextures(string materialId, string materialPath)
        {
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
            var material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
            if (material == null)
                throw new InvalidOperationException("Unity AI material generation did not produce a Material at " + materialPath);

            CopyTexture(material, material.HasProperty("_BaseMap") ? "_BaseMap" : "_MainTex", materialId + "_ai_albedo");
            CopyTexture(material, "_BumpMap", materialId + "_ai_normal");
            CopyTexture(material, "_OcclusionMap", materialId + "_ai_occlusion");
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
        }

        static void CopyTexture(Material material, string property, string outputBaseName)
        {
            if (!material.HasProperty(property))
                return;

            var texture = material.GetTexture(property);
            if (texture == null)
                return;

            var sourcePath = AssetDatabase.GetAssetPath(texture);
            if (string.IsNullOrEmpty(sourcePath))
                return;

            var extension = Path.GetExtension(sourcePath);
            if (string.IsNullOrEmpty(extension))
                extension = ".png";

            var destinationPath = SourceTextureFolder + "/" + outputBaseName + extension;
            if (AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(destinationPath) != null)
                AssetDatabase.DeleteAsset(destinationPath);

            if (!AssetDatabase.CopyAsset(sourcePath, destinationPath))
                throw new InvalidOperationException("Failed to promote Unity AI texture from " + sourcePath + " to " + destinationPath);
        }

        static Type FindType(string fullName)
        {
            var type = AppDomain.CurrentDomain
                .GetAssemblies()
                .Select(a => a.GetType(fullName, false))
                .FirstOrDefault(t => t != null);
            if (type == null)
                throw new InvalidOperationException("Could not find Unity AI type: " + fullName);
            return type;
        }

        static void EnsureFolderRecursive(string assetFolder)
        {
            var parts = assetFolder.Split('/');
            var current = parts[0];
            for (var i = 1; i < parts.Length; i++)
            {
                var next = current + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next))
                    AssetDatabase.CreateFolder(current, parts[i]);
                current = next;
            }
        }

        static string GetRequestPath(string fileName)
        {
            var unityProjectRoot = Directory.GetParent(Application.dataPath).FullName;
            var repoRoot = Directory.GetParent(unityProjectRoot).FullName;
            return Path.Combine(repoRoot, "build", "requests", fileName);
        }

        readonly struct MaterialRequest
        {
            public readonly string Id;
            public readonly string Prompt;

            public MaterialRequest(string id, string prompt)
            {
                Id = id;
                Prompt = prompt;
            }
        }
    }
}

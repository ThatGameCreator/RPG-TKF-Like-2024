using UnityEditor;
using UnityEditor.Overlays;
using UnityEngine.UIElements;
using System.Linq;

namespace Gyvr.Mythril2D
{
    [Overlay(typeof(SceneView), "Mythril2D Scene Selector", true)]
    public class SceneSelectorOverlay : Overlay
    {
        public override VisualElement CreatePanelContent()
        {
            var root = new VisualElement() { };

            string[] guids = AssetDatabase.FindAssets("t:scene", null);
            var scenes = guids.Select(guid => AssetDatabase.GUIDToAssetPath(guid)).ToArray();

            foreach (var scene in scenes)
            {
                var button = new Button() { text = scene.Split('/').Last() };
                button.clicked += () => UnityEditor.SceneManagement.EditorSceneManager.OpenScene(scene);
                root.Add(button);
            }

            return root;
        }
    }
}

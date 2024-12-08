using UnityEngine;

namespace Gyvr.Mythril2D
{
    [System.Serializable]
    public struct DialogueSequenceOption
    {
        public string name;
        public string nameKey;
        public DialogueSequence sequence;
        public DialogueMessage message;
    }

    [CreateAssetMenu(menuName = AssetMenuIndexer.Mythril2D_Dialogues + nameof(DialogueSequence))]
    public class DialogueSequence : DatabaseEntry
    {
        public string[] lines = null;
        public string[] lineKeys = null;
        public DialogueSequenceOption[] options = null;
        [SerializeReference, SubclassSelector]
        public ICommand toExecuteOnStart = null;
        [SerializeReference, SubclassSelector]
        public ICommand toExecuteOnCompletion = null;

        public DialogueTree ToDialogueTree(string speaker, params string[] args)
        {
            return DialogueUtils.CreateDialogueTree(this, speaker, args);
        }
    }
}

using UnityEngine;

namespace Gyvr.Mythril2D
{
    // 定义一个专门的类型继承自 DatabaseEntry
    public class StringDatabaseEntryReference : DatabaseEntry
    {
        // 可以在这里添加特定逻辑（例如附加信息），但当前只需要字符串
        public string guid;

        public StringDatabaseEntryReference(string guid)
        {
            this.guid = guid;
        }
    }
}

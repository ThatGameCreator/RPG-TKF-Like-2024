import os
import re


def extract_from_files(directory, output_file1, output_file2, file_extension, pattern):
    """
    提取指定格式文件中的特定内容并保存到输出文件。

    :param directory: 目标文件夹路径
    :param output_file1: 输出结果的文本文件路径
    :param file_extension: 要处理的文件扩展名（如 .asset）
    :param pattern: 正则表达式，用于提取目标内容
    """
    try:
        # 检查目录是否存在
        if not os.path.exists(directory):
            print(f"目录不存在: {directory}")
            return

        # 编译正则表达式
        regex = re.compile(pattern)

        # 打开输出文件
        with open(output_file1, 'w', encoding='utf-8') as out_file1:
            with open(output_file2, 'w', encoding='utf-8') as out_file2:
                # out_file1.write(f"提取自 {file_extension} 文件的内容：\n")
                # out_file1.write("=" * 50 + "\n")

                # 遍历文件夹中的文件
                # for root, _, files in os.walk(directory):
                #     for file in files:
                for file in os.listdir(directory):
                    if file.endswith(file_extension):  # 筛选特定格式的文件
                        # file_path = os.path.join(root, file)
                        file_path = directory + "\\" + file
                        # out_file1.write(f"\n文件: {file_path}\n")
                        # out_file1.write("-" * 50 + "\n")

                        # 打开特定文件并逐行处理
                        with open(file_path, 'r', encoding='utf-8') as asset_file:
                            for line in asset_file:
                                match = regex.search(line)
                                if match:
                                    # 提取正则匹配的内容
                                    content1 = match.group(1).strip()
                                    content2 = "id_" + content1.replace(" ","_").lower() + "_item"
                                    out_file1.write(f"{content1}\n")
                                    out_file2.write(f"{content2}\n")

        print(f"提取完成，结果已保存到: {output_file1}")
        print(f"提取完成，结果已保存到: {output_file2}")

    except Exception as e:
        print(f"发生错误: {e}")


# 示例用法
if __name__ == "__main__":
    # 输入目录路径(..\..=Assets)
    target_directory_base = r"..\..\Mythril2D\Demo\Database\Items"

    # 输出结果文件路径
    output_dir = r".\Items"
    output_txt_names = ["", "Equipments", "Jewelry", "Monster Drops", "Potions", "Quests"]
    output_txt_suffix1 = r"_en_name.txt"
    output_txt_suffix2 = r"_id.txt"

    # 文件扩展名
    file_ext = ".asset"

    # 正则表达式：提取 m_displayName 的值
    regex_pattern = r"m_displayName:\s*(.+)"

    # 调用函数
    for dirname in output_txt_names:
        output_txt1 = (output_dir + "\\" + "rootdir" + output_txt_suffix1) if dirname == "" else (
                output_dir + "\\" + dirname + output_txt_suffix1)
        output_txt2 = (output_dir + "\\" + "rootdir" + output_txt_suffix2) if dirname == "" else (
                output_dir + "\\" + dirname + output_txt_suffix2)
        target_directory = target_directory_base + "\\" + dirname
        extract_from_files(target_directory, output_txt1, output_txt2, file_ext, regex_pattern)

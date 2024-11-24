import os
import re


def extract_and_save(directory, output_file, pattern):
    """
    读取指定目录中的文件名，使用正则表达式提取信息，并保存结果到文本文件。

    :param directory: 文件夹路径
    :param output_file: 输出结果的文本文件路径
    :param pattern: 正则表达式，用于提取文件名信息
    """
    try:
        # 确保目标目录存在
        if not os.path.exists(directory):
            print(f"目录不存在: {directory}")
            return

        # 编译正则表达式
        regex = re.compile(pattern)
        output_file = output_file + r".txt"

        pattern_inner = r"^  m_displayName: (.+)$"
        regex_inner = re.compile(pattern_inner)

        # 打开输出文件
        with open(output_file, 'w', encoding='utf-8') as f:
            # f.write("文件名\t提取结果\n")  # 写入表头

            # 遍历目录中的文件
            for filename in os.listdir(directory):
                # 提取文件名，不包含扩展名
                file_base = os.path.splitext(filename)[0]

                # 使用正则表达式提取信息
                match = regex.search(file_base)
                if match:
                    print(filename)
                    # 提取正则匹配的内容
                    extracted_info = match.groups()[0]  # 提取命名组
                    filename_full = directory + "\\" +filename
                    # print(filename_full)
                    with open(filename_full,'r', encoding='utf-8') as f_source:
                        match_content = regex_inner.search(filename_full)
                        print(match_content)
                    # print(match.groups()[0])
                    f.write(f"{extracted_info}\n")

        print(f"结果已保存到: {output_file}")
    except Exception as e:
        print(f"发生错误: {e}")


# 示例用法
if __name__ == "__main__":
    # 目标文件夹(..\..=Assets)
    target_directory_base = r"..\..\Mythril2D\Demo\Database\Items"

    # 输出文件
    output_dir = r"."
    output_txt_filenames = ["Equipments", "Jewelry", "Monster Drops", "Potions", "Quests"]

    # 正则表达式
    regex_pattern = r"^ITEM_(?P<Name>.+).asset$"

    for dirname in output_txt_filenames:
        output_txt = output_dir + "\\" + dirname
        target_directory = target_directory_base + "\\" + dirname

        # 调用函数
        extract_and_save(target_directory, output_txt, regex_pattern)

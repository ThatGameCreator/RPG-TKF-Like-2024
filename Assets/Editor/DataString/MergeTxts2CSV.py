import csv


def merge_all_lines_from_text_files_to_csv(file_paths, output_csv):
    """
    将多个文本文档中的所有行对应合并为 CSV 文件中的多行。

    :param file_paths: 文本文档路径列表
    :param output_csv: 输出的 CSV 文件路径
    """
    # 打开文件并逐行读取数据
    data = [["Key", "English(en)", "Chinese (Simplified)(zh-Hans)"]]

    # 获取文件中行数最多的文件，用于对齐其他文件
    max_lines = 0
    file_contents = []

    for file_path in file_paths:
        try:
            with open(file_path, 'r', encoding='utf-8') as file:
                lines = file.readlines()
                file_contents.append(lines)
                max_lines = max(max_lines, len(lines))  # 记录最大行数
        except FileNotFoundError:
            print(f"File not found: {file_path}")
            file_contents.append([])

    # 遍历最大行数，将每个文件的对应行合并
    for i in range(max_lines):
        row = []
        for file_lines in file_contents:
            if i < len(file_lines):
                row.append(file_lines[i].strip())  # 去掉首尾空白
            else:
                row.append("")  # 如果该文件行数不够，填充空白
        data.append(row)

    # 写入 CSV 文件
    with open(output_csv, 'w', newline='', encoding='utf-8') as csv_file:
        writer = csv.writer(csv_file)
        writer.writerows(data)  # 写入所有行数据

    print(f"CSV file created: {output_csv}")


# 示例用法

root_dir = r"./Items/"
target_names = ["", "Equipments", "Jewelry", "Monster Drops", "Potions", "Quests"]
id_perfix = r"_id.txt"
en_perfix = r"_en_name.txt"
zh_perfix = r"_zh_name.txt"
perfixs = [id_perfix, en_perfix, zh_perfix]
for target_name in target_names:
    text_files = [root_dir + "rootdir" + str_tmp for str_tmp in perfixs] if target_name == "" else [root_dir + target_name + str_tmp for str_tmp in perfixs]
    # text_files = ["Equipments_id.txt", "Equipments_en_name.txt", "Equipments_zh_name.txt"]  # 替换为你的文件路径
    output_csv_file = (root_dir + "output_rootdir" + ".csv") if target_name == "" else (
                root_dir + "output_" + target_name + ".csv")

    merge_all_lines_from_text_files_to_csv(text_files, output_csv_file)

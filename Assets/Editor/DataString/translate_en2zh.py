from deep_translator import GoogleTranslator
import os
# import ssl
# import urllib3


def translate_file(input_file, output_file, target_language="zh-CN"):
    """
    将文本文件的每一行翻译为指定语言，并保存到新文件。

    :param input_file: 输入的文本文件路径
    :param output_file: 输出的文本文件路径
    :param target_language: 目标语言代码，默认中文 "zh-cn"
    """
    translator = GoogleTranslator(source='auto', target=target_language)

    with open(input_file, 'r', encoding='utf-8', errors='ignore') as infile, \
            open(output_file, 'w', encoding='utf-8') as outfile:

        for line in infile:
            line = line.strip()
            if line:
                try:
                    # 翻译文本
                    translated = translator.translate(line)
                    outfile.write(translated + '\n')
                except Exception as e:
                    print(f"翻译失败: {e}, 原文保留: {line}")
                    outfile.write(line + '\n')  # 出错时保留原文
            else:
                outfile.write('\n')  # 保留空行

    print(f"翻译完成，结果已保存到: {output_file}")


# 示例用法
if __name__ == "__main__":
    os.environ["http_proxy"] = "http://127.0.0.1:8889"
    os.environ["https_proxy"] = "http://127.0.0.1:8889"
    # 输入和输出文件路径
    target_dir = r".\Items"
    output_txt_names = ["", "Equipments", "Jewelry", "Monster Drops", "Potions", "Quests"]
    input_txt_suffix = r"_en_name.txt"
    output_txt_suffix = r"_zh_name.txt"
    for output_txt_name in output_txt_names:
        target_file = (target_dir + "\\" + "rootdir") if output_txt_name == "" else (target_dir + "\\" + output_txt_name)
        input_txt = target_file + input_txt_suffix
        output_txt = target_file + output_txt_suffix
        print(input_txt)
        print(output_txt)

        # 调用函数
        translate_file(input_txt, output_txt)

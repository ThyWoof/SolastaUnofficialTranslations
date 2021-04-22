#
# AUTHOR: Zappastuff - 2021-APR
#
# REQUIRES:
#   - Python 3.9.x
#   - deep_translator library (pip3 install deep_translator)
#

import argparse
import os
import re
import sys
from deep_translator import GoogleTranslator


MAX_CHARS = 5000
SEPARATOR = "\n"


def parse_command_line():
    my_parser = argparse.ArgumentParser(description='Translates Solasta game terms')
    my_parser.add_argument('file',
                        type=str,
                        help='file to translate')
    my_parser.add_argument('-c', '--code',
                        type=str,
                        help='translation language code')
    return my_parser.parse_args()


def progress(count, total, status=''):
    bar_len = 60
    filled_len = int(round(bar_len * count / float(total)))

    percents = round(100.0 * count / float(total), 1)
    bar = '=' * filled_len + '-' * (bar_len - filled_len)

    sys.stdout.write('[%s] %s%s ...%s\r' % (bar, percents, '%', status))
    sys.stdout.flush() 


def unpack_record(record):
    term = ""
    text = ""
    try:
        (term, text) = record.split(" ", 1)
        text = text.replace("\\n", "<n>").strip()
    except:
        term = record
    return term, text if text != "" else "EMPTY"


def get_translation_records(filename):
    with open(filename, "rt", encoding="utf-8") as f:
        record = f.readline()
        while record:
            yield unpack_record(record)
            record = f.readline()
            while record and record.strip() == "":
                record = f.readline()


def get_translation_chunks(filename, code):
    line_count = 0
    total_lines = sum(1 for line in open(filename))

    total_len = 0 
    terms = []
    texts = []
    for term, text in get_translation_records(filename):
        line_count = line_count + 1
        progress(line_count, total_lines, f" language {code}")
        total_len = total_len + len(text) + len(SEPARATOR)
        if total_len > MAX_CHARS:
            yield SEPARATOR.join(terms), SEPARATOR.join(texts)
            total_len = len(text) + len(SEPARATOR)
            terms = []
            texts = []
        terms.append(term)
        texts.append(text)
    yield SEPARATOR.join(terms), SEPARATOR.join(texts)
    print()


def translate_chunk(text, code):
    translated = GoogleTranslator(source="auto", target=code).translate(text) if len(text) <= MAX_CHARS else text
    return translated.replace("<n>", "\\n")


def fix_format(text):
    # <# ([A-F0-9]*)> (.*) </color> 
    # <#$1>$2</color>
    text = text.replace(" <", "<")
    text = text.replace(" >", ">")
    text = text.replace("<# ", "<#")
    text = text.replace("\\n ", "\\n")
    text = text.replace(" \\n", "\\n")
    return text


def translate(filename, code):
    with open(f"Translation-{code}.txt", "wt", encoding="utf-8") as f:
        for terms, texts in get_translation_chunks(filename, code):
            translated = translate_chunk(texts, code)
            translated = fix_format(translated)
            texts = translated.split(SEPARATOR)
            for term in terms.split(SEPARATOR):
                f.write(f"{term} {texts.pop(0)}\n")
            f.flush()


def main():
    args = parse_command_line()
    code = 'pt' if args.code is None else args.code
    translate(args.file, code)


if __name__ == "__main__":
    main()
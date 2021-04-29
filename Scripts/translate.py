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
from deep_translator import GoogleTranslator, MicrosoftTranslator, DeepL


CHARS_MAX = 5000
SEPARATOR = "\n"
ENGINES = {
    'Google': GoogleTranslator,
    'Microsoft': MicrosoftTranslator
}


def parse_command_line():
    my_parser = argparse.ArgumentParser(description='Translates Solasta game terms')
    my_parser.add_argument('input',
                        type=str,
                        help='input file to translate')
    my_parser.add_argument('-c', '--code',
                        type=str,
                        required=True,
                        help='language code to translate to')
    my_parser.add_argument('-d', '--dict',
                        type=str,
                        help='dictionary file to fix auto translation')
    my_parser.add_argument('-k', '--api-key',
                        type=str,
                        help='api key for engines that require it')
    my_parser.add_argument('-e', '--engine',
                        type=str,
                        choices=['Microsoft', 'Google', 'DeepL'],
                        default='Google',
                        help='translator engine to use')

    return my_parser.parse_args()


def load_dictionary(filename):
    dictionary = {}
    if not filename:
       pass
    elif not os.path.exists(filename):
        print(f"WARNING: dictionary file doesn't exist. using an empty one")
    else:
        with open(filename, "rt", encoding="utf-8") as f:
            record = "\n"
            while record:
                record = f.readline()
                if record and record.split():
                    try:
                        (f, r) = record.split(" ", 1).strip()
                        dictionary[f] = r
                    except:
                        print(f"ERROR: skipping dictionary line {record}")

    return dictionary


def display_progress(count, total, status=''):
    bar_len = 80
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
        text = text.strip()
    except:
        term = record

    return term, text if text != "" else "EMPTY"


def get_records(filename):
    try:
        line_count = 0
        line_total = sum(1 for line in open(filename))
        with open(filename, "rt", encoding="utf-8") as f:
            record = "\n"
            while record:
                line_count += 1
                display_progress(line_count, line_total)
                record = f.readline()
                if record and record.split():
                    yield unpack_record(record)

    except FileNotFoundError:
        print("ERROR")


def get_chunks(filename):
    chars_len = 0 
    terms = []
    texts = []
    for term, text in get_records(filename):
        chars_len += len(text) + len(SEPARATOR)
        if chars_len > CHARS_MAX:
            yield SEPARATOR.join(terms), SEPARATOR.join(texts)
            chars_len = len(text) + len(SEPARATOR)
            terms = []
            texts = []

        terms.append(term)
        texts.append(text)

    yield SEPARATOR.join(terms), SEPARATOR.join(texts)


def translate_chunk(engine, text, code):
    l = ["<b>", "#B", "<i>", "#C", "</b>", "`B", "</i>", "`", "\\n", "#N", "</color>", "#C"]

    for i in range(0, len(l), 2):
        text = text.replace(l[i], l[i+1])

    translated = engine(source="auto", target=code).translate(text) if len(text) <= CHARS_MAX else text

    for i in range(0, len(l), 2):
        translated = translated.replace(l[i+1], l[i])

    return translated


def apply_dictionary(dictionary, text):
    # r = re.compile(r"<# ([A-F0-9]*)> (.*) </color>")
    # text = r.sub(r"<#\1>\2</color>", text)

    # r = re.compile(r"<i> (.*) </i>")
    # text = r.sub(r"<i>$1</i>", text)

    # r = re.compile(r"<b> (.*) </b>")
    # text = r.sub(r"<b>$1</b>", text)

    # text = text.replace("</color> ", "</color>")

    # text = text.replace("\\ N", "\\n")
    # text = text.replace("\\ n", "\\n")
    # text = text.replace("\\n ", "\\n")
    # text = text.replace(" \\n", "\\n")

    # for key in dictionary:
    #     text = text.replace(key, dictionary[key])

    return text


def translate(input, output, code, engine, dictionary=None, api_key=None):
    with open(output, "wt", encoding="utf-8") as f:
        for terms, texts in get_chunks(input):
            translated = translate_chunk(engine, texts, code)
            replaced = apply_dictionary(dictionary, translated)
            replaceds = replaced.split(SEPARATOR)
            for term in terms.split(SEPARATOR):
                f.write(f"{term} {replaceds.pop(0)}\n")


def main():
    args = parse_command_line()
    translate(
        args.input, 
        f"Translation-{args.code}.txt",
        args.code, 
        ENGINES[args.engine], 
        load_dictionary(args.dict),
        args.api_key)


if __name__ == "__main__":
    main()
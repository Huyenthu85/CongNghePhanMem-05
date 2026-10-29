
import sys
from deep_translator import GoogleTranslator
from deep_translator.exceptions import TooManyRequests

def main():
    if len(sys.argv) != 2:
        raise ValueError("Can cung cap ma ngon ngu.")

    targets = {
        "en": "en",
        "zh": "zh-CN",
        "km": "km"
    }
    language = sys.argv[1]

    if language not in targets:
        raise ValueError("Ngon ngu khong duoc ho tro.")

    text = sys.stdin.read().strip()
    if not text:
        raise ValueError("Noi dung khong duoc de trong.")

    try:
        result = GoogleTranslator(
            source="vi",
            target=targets[language]
        ).translate(text)

        if not result or not result.strip():
            raise RuntimeError("Khong nhan duoc ban dich.")

        sys.stdout.write(result.strip())
    except TooManyRequests:
        sys.stderr.write(
            "RATE_LIMIT: Dich vu dich may dang gioi han yeu cau.\n"
        )
        sys.exit(2)

if __name__ == "__main__":
    try:
        main()
    except Exception as e:
        sys.stderr.write(str(e) + "\n")
        sys.exit(1)

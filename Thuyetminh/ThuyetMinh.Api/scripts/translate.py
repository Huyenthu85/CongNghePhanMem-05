import sys
import html
import time
import requests
from deep_translator import GoogleTranslator

TARGETS = {
    "en": "en",
    "zh": "zh-CN",
    "km": "km"
}


def split_text(text, max_bytes=480):
    chunks = []
    current = ""

    for word in text.split():
        candidate = (current + " " + word).strip()

        if len(candidate.encode("utf-8")) <= max_bytes:
            current = candidate
        else:
            if current:
                chunks.append(current)

            if len(word.encode("utf-8")) > max_bytes:
                raise ValueError("Tu qua dai de dich.")

            current = word

    if current:
        chunks.append(current)

    return chunks


def validate(result, source):
    if not isinstance(result, str) or not result.strip():
        raise RuntimeError("Khong nhan duoc ban dich.")

    result = html.unescape(result).strip()

    if result.casefold() == source.strip().casefold():
        raise RuntimeError("Noi dung chua duoc dich.")

    return result


def translate_google(text, target):
    time.sleep(6)
    result = GoogleTranslator(
        source="vi",
        target=target
    ).translate(text)

    return validate(result, text)


def translate_mymemory(text, target):
    results = []

    for chunk in split_text(text):
        time.sleep(2)
        response = requests.get(
            "https://api.mymemory.translated.net/get",
            params={
                "q": chunk,
                "langpair": "vi|" + target
            },
            timeout=10
        )

        response.raise_for_status()
        data = response.json()

        if (
            str(data.get("responseStatus")) != "200"
            or data.get("quotaFinished")
        ):
            raise RuntimeError(
                data.get("responseDetails")
                or "MyMemory request failed."
            )

        translated = data["responseData"]["translatedText"]
        results.append(html.unescape(translated).strip())

    return validate(" ".join(results), text)


def main():
    if len(sys.argv) != 2:
        raise ValueError("Can cung cap ma ngon ngu.")

    language = sys.argv[1]

    if language not in TARGETS:
        raise ValueError("Ngon ngu khong duoc ho tro.")

    text = sys.stdin.read().strip()

    if not text:
        raise ValueError("Noi dung khong duoc de trong.")

    target = TARGETS[language]

    try:
        result = translate_google(text, target)
        sys.stderr.write("PROVIDER: GoogleTranslator\n")

    except Exception as google_error:
        try:
            result = translate_mymemory(text, target)
            sys.stderr.write("PROVIDER: MyMemory\n")

        except Exception as memory_error:
            raise RuntimeError(
                f"Google: {google_error}; "
                f"MyMemory: {memory_error}"
            )

    sys.stdout.write(result)


if __name__ == "__main__":
    try:
        main()
    except Exception as error:
        sys.stderr.write(str(error) + "\n")
        sys.exit(1)
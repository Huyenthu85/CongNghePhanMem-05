
import asyncio
import json
import os
import sys
import tempfile
from pathlib import Path

import edge_tts
from gtts import gTTS
from mutagen.mp3 import MP3

VOICES = {
    "en": "en-US-JennyNeural",
    "zh": "zh-CN-XiaoxiaoNeural"
}

async def generate(text, language, output):
    if language == "vi":
        await asyncio.to_thread(
            lambda: gTTS(text=text, lang="vi").save(str(output))
        )
    elif language == "km":
        await asyncio.to_thread(
            lambda: gTTS(text=text, lang="km").save(str(output))
        )
    else:
        communicate = edge_tts.Communicate(
            text=text,
            voice=VOICES[language]
        )
        await communicate.save(str(output))

async def main():
    if len(sys.argv) != 3:
        raise ValueError("Usage: tts.py <language> <output.mp3>")

    language = sys.argv[1]
    output = Path(sys.argv[2]).resolve()

    if language not in ("vi", "en", "zh", "km"):
        raise ValueError("Unsupported language")

    if output.suffix.lower() != ".mp3":
        raise ValueError("Output must be an MP3 file")

    text = sys.stdin.read().strip()
    if not text:
        raise ValueError("Narration text is empty")

    output.parent.mkdir(parents=True, exist_ok=True)

    fd, temporary_name = tempfile.mkstemp(
        suffix=".mp3",
        dir=str(output.parent)
    )
    os.close(fd)
    temporary = Path(temporary_name)

    try:
        await generate(text, language, temporary)

        audio = MP3(str(temporary))
        duration = int(audio.info.length + 0.999)

        if duration <= 0:
            raise ValueError("Invalid audio duration")

        temporary.replace(output)

        print(json.dumps({
            "filePath": str(output),
            "durationSeconds": duration
        }))
    finally:
        if temporary.exists():
            temporary.unlink()

if __name__ == "__main__":
    try:
        asyncio.run(main())
    except Exception as e:
        print(str(e), file=sys.stderr)
        sys.exit(1)

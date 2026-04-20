"""
analyze_feedback.py
===================
Reads the UserTestResultsWave2 file (.xlsx or .csv), analyses free-text
responses from question 20:
    - Q20: "What did you like most and less about the game."

Each response is matched against thematic categories. The summary table
is followed by a breakdown showing each response and the category it
was assigned to.
"""

import sys
from collections import defaultdict
from pathlib import Path

sys.path.insert(0, str(Path(__file__).parent))
from analyze_suggestions import (  # noqa: E402
    UNCATEGORIZED_LABEL,
    FILE_PATH,
    load_file,
    match_themes,
)

Q20_SUBSTRING = "like most"
NON_ANSWERS = {"no", "nop", "no.", "none", "-", "n/a", "", "nan"}
W = 62


def find_column(columns: list[str], substring: str) -> str | None:
    for col in columns:
        if substring.lower() in col.lower():
            return col
    return None


def analyse_column(df, col: str) -> tuple[list[dict], int]:
    total = 0
    theme_to_participants: dict[str, set] = defaultdict(set)
    theme_to_responses: dict[str, list[tuple[int, str]]] = defaultdict(list)

    for idx, (_, row) in enumerate(df.iterrows(), start=1):
        val = str(row.get(col, "") or "").strip()
        if not val or val.lower() in NON_ANSWERS:
            continue
        total += 1

        themes = match_themes(val)
        if themes == [UNCATEGORIZED_LABEL]:
            continue

        for t in themes:
            theme_to_participants[t].add(idx)
            theme_to_responses[t].append((idx, val))

    records = []
    for theme_label, participants in theme_to_participants.items():
        if theme_label.startswith("Suggestion:"):
            continue
        count = len(participants)
        pct = round(count / total * 100, 1) if total else 0.0
        records.append({
            "Theme": theme_label,
            "Participants": count,
            "Percentage (%)": pct,
            "Responses": theme_to_responses[theme_label],
        })

    records.sort(key=lambda r: r["Participants"], reverse=True)
    return records, total


def _wrap_print(text: str, indent: str = "    ") -> None:
    words = text.split()
    line = indent
    for word in words:
        if len(line) + len(word) + 1 > W + 8:
            print(line)
            line = indent + word
        else:
            line += (" " if line.strip() else "") + word
    if line.strip():
        print(line)


def print_question_report(question_label: str, col_name: str, records: list[dict], total: int) -> None:
    print(f"\n{'='*W}")
    print(f"  {question_label}")
    print(f"  Coluna: \"{col_name}\"")
    print(f"  (de {total} participantes com pelo menos uma resposta)")
    print(f"{'='*W}")

    if not records:
        print("  (Nenhum tema identificado)")
        print(f"\n{'='*W}\n")
        return

    print(f"{'#':<4} {'Theme':<50} {'n':>4}  {'%':>6}")
    print(f"{'-'*4} {'-'*50} {'-'*4}  {'-'*6}")
    for i, row in enumerate(records, start=1):
        theme = row["Theme"]
        parts = [theme[j:j+50] for j in range(0, len(theme), 50)]
        for k, part in enumerate(parts):
            if k == 0:
                print(f"{i:<4} {part:<50} {row['Participants']:>4}  {row['Percentage (%)']:>5.1f}%")
            else:
                print(f"{'':4} {part:<50}")

    # ── Per-theme response breakdown ────────────────────────────────────────
    print(f"\n{'─'*W}")
    print(f"  Respostas por categoria")
    print(f"{'─'*W}")

    for row in records:
        print(f"\n  >> {row['Theme']}")
        for participant, response in row["Responses"]:
            print(f"\n     Participante #{participant}")
            _wrap_print(response)

    print(f"\n{'='*W}\n")


def main(file_path: str = FILE_PATH) -> None:
    df = load_file(file_path)
    columns = df.columns.tolist()

    q20_col = find_column(columns, Q20_SUBSTRING)
    if not q20_col:
        raise RuntimeError(
            f"Coluna da Pergunta 20 não encontrada (substring: '{Q20_SUBSTRING}').\n"
            f"Colunas disponíveis: {columns}"
        )

    records, total = analyse_column(df, q20_col)
    print_question_report("Pergunta 20 -> O que gostou mais e menos do jogo", q20_col, records, total)


if __name__ == "__main__":
    main(FILE_PATH)
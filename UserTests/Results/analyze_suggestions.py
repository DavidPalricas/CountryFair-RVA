"""
analyze_suggestions.py
======================
Reads the UserTestResultsWave2 file (.xlsx or .csv), extracts open-ended
responses from the free-text column for question 22:
    - "Other Suggestions"

Each response is matched against thematic categories via regex patterns.
A single response can belong to multiple themes simultaneously, since
suggestions often address several topics at once. Each participant is
counted at most once per theme.

The report shows how many participants mentioned each theme and the
corresponding percentage of the total respondent pool (participants with
at least one non-empty response in question 22).
"""

import csv
import io
import re
from collections import defaultdict
from pathlib import Path

import pandas as pd


# ---------------------------------------------------------------------------
# FILE PATH CONSTANT
# Supports .xlsx or .csv — extension is detected automatically.
# ---------------------------------------------------------------------------
FILE_PATH = "UserTestResultsWave2.csv"

# ---------------------------------------------------------------------------
# FREE-TEXT COLUMNS TO ANALYSE
# Partial name matches (case-insensitive) against actual column headers.
# ---------------------------------------------------------------------------
TEXT_COLUMN_SUBSTRINGS = [
    "suggestions, comments",  # Question 21
    "Other Suggestions",      # Question 22
]

# ---------------------------------------------------------------------------
# THEME DEFINITIONS
# Each theme has a label and a list of keyword/phrase regex patterns.
# A response is assigned to a theme if ANY pattern matches (case-insensitive).
# Order matters: more-specific themes must come before broader ones.
# ---------------------------------------------------------------------------
THEMES: list[dict] = [

    # ------------------------------------------------------------------
    # UI / READABILITY
    # ------------------------------------------------------------------
    {
        "label": "Suggestion: audio narration / voice-over for instructions",
        "patterns": [
            r"narrad", r"narrat", r"audio.*instrução", r"instrução.*audio",
            r"audio.*instru", r"instru.*audio",
            r"voice.*over", r"voiced",
            r"audio.*oposto.*texto",
            r"video.*introd", r"introd.*video",
            r"dita.*audio", r"formato.*audio",
            r"aconcelho.*audio",
        ],
    },


    # ------------------------------------------------------------------
    # FRISBEE MECHANICS
    # ------------------------------------------------------------------
    {
        "label": "Suggestion: frisbee trajectory indicator",
        "patterns": [
            r"trajectory.*frisbee", r"frisbee.*trajectory",
            r"indicação.*trajetória", r"trajetória.*frisbee",
            r"indication.*trajectory", r"trajectory.*indication",
            r"guide.*frisbee", r"frisbee.*guide",
        ],
    },
    {
        "label": "Suggestion: frisbee scoring zones redesign (darts-style / distance)",
        "patterns": [
            r"darts.*target", r"darts.like", r"target.*frisbee.*scoring",
            r"scoring.*region", r"smaller.*region",
            r"zonas.*laranja.*mais longe", r"distância.*balões.*variar",
            r"vary.*distance.*balloon",
        ],
    },

    # ------------------------------------------------------------------
    # ARCHERY MECHANICS
    # ------------------------------------------------------------------
    {
        "label": "Suggestion: more archery space / obstacle variation",
        "patterns": [
            r"espaço.*maior.*arco", r"arco.*espaço.*maior",
            r"distância.*player.*balões", r"obstáculo.*arco",
            r"obstáculos.*jogador", r"vary.*angle", r"ângulo.*lançamento",
            r"greater range.*control.*archery", r"range.*archery",
        ],
    },
    {
        "label": "Suggestion: arrow guide on/off toggle",
        "patterns": [
            r"switch.*on.*off.*guide.*arrow", r"toggle.*guide.*arrow",
            r"guide.*arrow.*on.*off", r"arrow.*guide.*optional",
            r"personaliz.*arrow.*guide",
            r"switching.*on.*off.*guide.*arrow",
        ],
    },

    # ------------------------------------------------------------------
    # ADAPTIVE DIFFICULTY
    # ------------------------------------------------------------------
    {
        "label": "Suggestion: better / explicit feedback on difficulty change",
        "patterns": [
            r"feedback.*dificuldade", r"dificuldade.*feedback",
            r"level.*up.*sound", r"som.*level.*up", r"fogos.*artificio",
            r"fireworks", r"sound.*difficulty", r"efeito.*sonoro.*mudança",
            r"mudança.*dificuldade.*som", r"more.*feedback.*difficult",
            r"subida.*nível.*feedback",
            r"explicit.*say.*difficulty.*increasing",
            r"explicitly.*say.*difficulty",
            r"reward.*performance", r"recompensa.*progresso",
            r"see.*evolution.*difficulty", r"difficulty.*evolution",
        ],
    },
    {
        "label": "Suggestion: more gradual difficulty adjustment",
        "patterns": [
            r"mais gradual", r"gradual.*ajuste", r"ajuste.*gradual",
            r"more.*gradual", r"gradual.*adjust",
        ],
    },

    # ------------------------------------------------------------------
    # AUDIO / SOUND
    # ------------------------------------------------------------------
    {
        "label": "Suggestion: more audio feedback / dialogue",
        "patterns": [
            r"mais.*feedback.*auditivo", r"feedback.*auditivo.*errar",
            r"mais.*diálogo", r"more.*dialogue", r"more.*audio.*feedback",
            r"audio.*feedback.*miss",
        ],
    },



    # ------------------------------------------------------------------
    # MORE MINI-GAMES / CONTENT
    # ------------------------------------------------------------------
    {
        "label": "Suggestion: more mini-games / maps / content variety",
        "patterns": [
            r"more.*mini.game", r"mais.*jogos", r"mais.*mini.jogo",
            r"diversidade.*jogos", r"more.*game.*mode",
            r"other.*mode", r"outros.*modos",
            r"more.*maps", r"different.*maps",
            r"collection.*maps", r"cover more.*needs",
            r"alargar.*mini.jogos", r"mais variedade.*mini.jogo",
            r"variedade.*mini.jogo",
        ],
    },

    # ------------------------------------------------------------------
    # SCORING / PROGRESSION
    # ------------------------------------------------------------------
    {
        "label": "Suggestion: highscore / competitive / progression mode",
        "patterns": [
            r"highscore", r"high.*score",
            r"acréscimo.*pontos", r"register.*highscore",
            r"limited.*time.*mode", r"time.*mode",
            r"see.*evolution.*score", r"compare.*evolution",
            r"evolution.*difficulty.*end.*game",
            r"feedback.*end.*game", r"scores.*compare",
        ],
    },

    # ------------------------------------------------------------------
    # UI LAYOUT / ELEMENTS
    # ------------------------------------------------------------------
    {
        "label": "Suggestion: UI elements closer / better positioned",
        "patterns": [
            r"ui.*closer", r"score.*closer", r"score.*centred",
            r"elementos.*ui.*mais.*perto", r"elementos.*ui.*centrad",
            r"score.*cor.*balão.*mais perto", r"botões.*altura.*jogador",
            r"buttons.*height", r"altura.*botões",
            r"na mesma área.*visão", r"same.*field.*view",
            r"notification.*bell", r"bell.*notification",
            r"objects.*same.*area.*vision",
        ],
    },

    # ------------------------------------------------------------------
    # LEFT-HANDEDNESS / MOTOR ACCESSIBILITY
    # ------------------------------------------------------------------
    {
        "label": "Suggestion: left-handed / motor accessibility support",
        "patterns": [
            r"left.hand", r"esquerdist", r"adaptar.*esquerdist",
            r"left.*handed", r"esquerda.*dificuldade",
            r"dificuldade.*esquerda", r"movimentos.*esquerda",
        ],
    },

    # ------------------------------------------------------------------
    # AVATAR / HANDS
    # ------------------------------------------------------------------
    {
        "label": "Suggestion: avatar / hand customisation",
        "patterns": [
            r"customiz.*avatar", r"customiz.*mão", r"customizar.*avatar",
            r"avatar.*customiz", r"hand.*customiz",
            r"personalizar.*mão",
        ],
    },

]

UNCATEGORIZED_LABEL = "Other / uncategorized"


# ---------------------------------------------------------------------------
# FILE LOADER – supports .xlsx and .csv
# ---------------------------------------------------------------------------

def load_file(file_path: str) -> pd.DataFrame:
    """
    Load either an .xlsx or .csv file into a DataFrame.

    For CSV: uses Python's csv module to tolerate rows with more fields than
    the header (unescaped commas in free-text columns).
    For XLSX: uses openpyxl via pandas read_excel.
    """
    path = Path(file_path)
    suffix = path.suffix.lower()

    if suffix == ".xlsx":
        df = pd.read_excel(path, engine="openpyxl", header=0)
        df.columns = [str(c).strip() for c in df.columns]
        df = df[df.iloc[:, 0].notna() & df.iloc[:, 0].astype(str).str.strip().ne("")]
        return df.reset_index(drop=True)

    elif suffix == ".csv":
        raw_text = path.read_text(encoding="utf-8-sig")
        reader = csv.reader(io.StringIO(raw_text))
        rows = list(reader)
        if not rows:
            raise ValueError(f"Empty file: {file_path}")
        header = [col.strip() for col in rows[0]]
        n_cols = len(header)
        records = []
        for row in rows[1:]:
            if not row or not row[0].strip():
                continue
            padded = (row + [""] * n_cols)[:n_cols]
            records.append({header[i]: padded[i].strip() for i in range(n_cols)})
        return pd.DataFrame(records)

    else:
        raise ValueError(f"Unsupported file format: {suffix}. Use .xlsx or .csv")


# ---------------------------------------------------------------------------
# HELPERS
# ---------------------------------------------------------------------------

def resolve_text_columns(columns: list[str]) -> list[str]:
    """
    Return the actual column names that contain any of the
    TEXT_COLUMN_SUBSTRINGS (case-insensitive partial match).
    """
    matched = []
    for col in columns:
        for sub in TEXT_COLUMN_SUBSTRINGS:
            if sub.lower() in col.lower():
                matched.append(col)
                break
    return matched


def collect_responses(row: pd.Series, text_cols: list[str]) -> list[str]:
    """
    Gather all non-empty free-text responses from a participant row.
    Skips values that are just whitespace or trivial non-answers.
    """
    non_answers = {"no", "nop", "no.", "none", "-", "n/a", "", "nan"}
    responses = []
    for col in text_cols:
        val = str(row.get(col, "") or "").strip()
        if val and val.lower() not in non_answers:
            responses.append(val)
    return responses


def match_themes(response: str) -> list[str]:
    """
    Return all theme labels whose patterns match the response.
    A response can belong to multiple themes simultaneously.
    """
    matched = []
    obs_lower = response.lower()
    for theme in THEMES:
        for pattern in theme["patterns"]:
            if re.search(pattern, obs_lower):
                matched.append(theme["label"])
                break
    return matched if matched else [UNCATEGORIZED_LABEL]


# ---------------------------------------------------------------------------
# MAIN ANALYSIS
# ---------------------------------------------------------------------------

def analyse(file_path: str) -> tuple[pd.DataFrame, int]:
    """
    Load the file, analyse free-text responses, and return a summary DataFrame.

    Each participant is counted at most once per theme, regardless of how many
    of their responses matched that theme.

    Returns
    -------
    tuple[pd.DataFrame, int]
        Summary table with columns (Theme | Participants | Percentage)
        and the total number of respondents with at least one free-text answer.
    """
    df = load_file(file_path)

    text_cols = resolve_text_columns(df.columns.tolist())
    if not text_cols:
        raise RuntimeError(
            "Could not find free-text columns. "
            f"Check TEXT_COLUMN_SUBSTRINGS against headers:\n{df.columns.tolist()}"
        )

    print(f"\n{'='*62}")
    print(f"  Total rows loaded       : {len(df)}")
    print(f"  Free-text columns found :")
    for col in text_cols:
        print(f"    • {col}")
    print(f"{'='*62}\n")

    non_answers = {"no", "nop", "no.", "none", "-", "n/a", "", "nan"}

    def has_real_response(row: pd.Series) -> bool:
        return any(
            str(row.get(col, "") or "").strip().lower() not in non_answers
            for col in text_cols
        )

    total_with_response = int(df.apply(has_real_response, axis=1).sum())

    theme_to_participants: dict[str, set] = defaultdict(set)
    theme_to_responses: dict[str, list[tuple[int, str]]] = defaultdict(list)

    for idx, row in df.iterrows():
        responses = collect_responses(row, text_cols)
        if not responses:
            continue
        themes_for_participant: set[str] = set()
        for response in responses:
            for theme_label in match_themes(response):
                themes_for_participant.add(theme_label)
                theme_to_responses[theme_label].append((int(idx) + 1, response))
        for theme_label in themes_for_participant:
            theme_to_participants[theme_label].add(idx)

    records = []
    for theme_label, participants in theme_to_participants.items():
        if theme_label == UNCATEGORIZED_LABEL:
            continue
        count = len(participants)
        pct = round(count / total_with_response * 100, 1)
        records.append({
            "Theme": theme_label,
            "Participants": count,
            "Percentage (%)": pct,
            "Responses": theme_to_responses[theme_label],
        })

    summary = pd.DataFrame(records)
    summary.sort_values("Participants", ascending=False, inplace=True)
    summary.reset_index(drop=True, inplace=True)

    return summary, total_with_response


def _wrap_print(text: str, indent: str = "    ") -> None:
    W = 62
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


def print_report(summary: pd.DataFrame, total: int) -> None:
    """Pretty-print the full summary table followed by per-theme responses."""
    W = 62
    print(f"\n{chr(61)*W}")
    print(f"  SUGGESTION THEME ANALYSIS — Questions 21 & 22")
    print(f"  (out of {total} participants with at least one response)")
    print(f"{chr(61)*W}")
    print(f"{chr(35):<4} {chr(84)+'heme':<50} {'n':>4}  {'%':>6}")
    print(f"{'-'*4} {'-'*50} {'-'*4}  {'-'*6}")

    for i, row in summary.iterrows():
        theme = row["Theme"]
        parts = [theme[j:j+50] for j in range(0, len(theme), 50)]
        for idx, part in enumerate(parts):
            if idx == 0:
                print(
                    f"{i+1:<4} {part:<50} "
                    f"{row['Participants']:>4}  {row['Percentage (%)']:>5.1f}%"
                )
            else:
                print(f"{'':4} {part:<50}")

    print(f"\n{'─'*W}")
    print(f"  Respostas por categoria")
    print(f"{'─'*W}")

    for _, row in summary.iterrows():
        print(f"\n  >> {row['Theme']}")
        for participant, response in row["Responses"]:
            print(f"\n     Participante #{participant}")
            _wrap_print(response)

    print(f"\n{chr(61)*W}\n")


# ---------------------------------------------------------------------------
# ENTRY POINT
# ---------------------------------------------------------------------------

if __name__ == "__main__":
    summary, total = analyse(FILE_PATH)
    print_report(summary, total)
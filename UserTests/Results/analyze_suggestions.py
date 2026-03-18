"""
analyze_suggestions.py
======================
Reads the UserTestResultsWave2.csv file, extracts open-ended responses from
three free-text columns:
    - "What did you like most and less about the game."
    - "Do you have any suggestions, comments, or thoughts about your
       experience with the personalized XR session?"
    - "Other Suggestions"

Each response is matched against thematic categories via regex patterns.
The report shows how many participants mentioned each theme and the
corresponding percentage of the total respondent pool.

Usage:
    python analyze_suggestions.py

Dependencies: pandas (pip install pandas)
"""

import csv
import io
import re
from collections import defaultdict
from pathlib import Path

import pandas as pd


# ---------------------------------------------------------------------------
# FILE PATH CONSTANT
# Change this to point to your CSV file.
# ---------------------------------------------------------------------------
FILE_PATH = "UserTestResultsWave2.csv"

# ---------------------------------------------------------------------------
# FREE-TEXT COLUMNS TO ANALYSE
# Partial name matches are used so minor whitespace/encoding differences in
# the header are tolerated.
# ---------------------------------------------------------------------------
TEXT_COLUMN_SUBSTRINGS = [
    "like most",
    "suggestions, comments",
    "Other Suggestions",
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
        "label": "Text / instructions hard to read or too long",
        "patterns": [
            r"text.*difficult", r"diffi.*read", r"hard.*read",
            r"letra.*maior", r"tamanho.*letra", r"font",
            r"caixas.*texto", r"text.*box", r"text.*not.*visible",
            r"texto.*n.o.*bastante.*vis",
            r"reduce.*text", r"shorten.*text", r"simplif.*text",
            r"instru.*audio", r"narrad", r"narrat",
            r"instrução.*fonte", r"fonte.*instrução",
            r"dificuldade.*ler", r"dificuldade a ler",
            r"aproxima.*caixas", r"distância.*caixas",
            r"diminuir.*distância.*caixas",
            r"closer.*text", r"text.*closer",
        ],
    },
    {
        "label": "Suggestion: audio narration / voice-over for instructions",
        "patterns": [
            r"narrad", r"narrat", r"audio.*instrução", r"instrução.*audio",
            r"audio.*instru", r"instru.*audio",
            r"voice.*over", r"voiced", r"text.*audio",
            r"audio.*text", r"audio.*oposto.*texto",
            r"video.*introd", r"introd.*video",
        ],
    },

    # ------------------------------------------------------------------
    # MENU / SELECTION INTERACTION
    # ------------------------------------------------------------------
    {
        "label": "Menu / game selection button unintuitive or buggy",
        "patterns": [
            r"menu.*bug", r"main menu.*bug", r"menu.*bugged",
            r"button.*appear", r"olhar.*jogo.*botão", r"look.*tent.*button",
            r"select.*button.*not.*intuitive", r"not.*intuitive.*select",
            r"não.*intuitivo.*olhar", r"olhar.*botão",
            r"não.*intuitivo.*ter de olhar",
            r"button.*hide", r"menu.*hide", r"shouldn.t hide",
            r"detection.*not.*great", r"jarring",
        ],
    },

    # ------------------------------------------------------------------
    # FRISBEE MECHANICS
    # ------------------------------------------------------------------
    {
        "label": "Frisbee mechanics / physics feel awkward or inconsistent",
        "patterns": [
            r"frisbee.*physics", r"frisbee.*weird", r"frisbee.*awkward",
            r"frisbee.*inconsistent", r"lançamento.*estranho",
            r"frisbee.*too fast", r"frisbee.*fast",
            r"disco.*secante", r"lançamento.*disco",
            r"frisbee.*not.*respond", r"frisbee.*doesn.t.*respond",
            r"frisbee.*às vezes.*n.o.*responde",
            r"hard.*throw.*frisbee", r"hard.*accurately.*throw",
            r"difficult.*release.*frisbee",
        ],
    },
    {
        "label": "Suggestion: frisbee trajectory indicator",
        "patterns": [
            r"trajectory.*frisbee", r"frisbee.*trajectory",
            r"indicação.*trajetória", r"trajetória.*frisbee",
            r"indication.*trajectory", r"trajectory.*indication",
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
        "label": "Archery target color not obvious / hard to notice",
        "patterns": [
            r"target.*color.*not.*obvious", r"color.*not.*obvious",
            r"color.*target.*changed", r"cor.*balão.*distrai",
            r"cor.*objetivo.*n.o.*vis", r"cor.*não.*perceb",
            r"objetivo.*jogo.*atual.*mais.*vis",
            r"make.*color.*more.*obvious", r"make.*target.*visible",
            r"não.*perceb.*logo.*balões", r"color.*had.*changed",
            r"wasn.t.*obvious.*color", r"not.*obvious.*target.*color",
            r"balões.*uns.*cima.*outros", r"balloons.*on.*top",
        ],
    },
    {
        "label": "Suggestion: more archery space / obstacle variation",
        "patterns": [
            r"espaço.*maior.*arco", r"arco.*espaço.*maior",
            r"distância.*player.*balões", r"obstáculo.*arco",
            r"obstáculos.*jogador", r"vary.*angle", r"ângulo.*lançamento",
        ],
    },

    # ------------------------------------------------------------------
    # ADAPTIVE DIFFICULTY
    # ------------------------------------------------------------------
    {
        "label": "Positive feedback on adaptive difficulty",
        "patterns": [
            r"ajuste.*automático", r"dificuldade.*ajust",
            r"ajust.*dificuldade", r"difficulty.*adjust",
            r"adapt.*difficulty", r"dificuldade.*crescente",
            r"increasing.*difficulty", r"difficulty.*increasing",
            r"dificuldade.*foi aumentando",
        ],
    },
    {
        "label": "Suggestion: better feedback on difficulty level-up",
        "patterns": [
            r"feedback.*dificuldade", r"dificuldade.*feedback",
            r"level.*up.*sound", r"som.*level.*up", r"fogos.*artificio",
            r"fireworks", r"sound.*difficulty", r"efeito.*sonoro.*mudança",
            r"mudança.*dificuldade.*som", r"more.*feedback.*difficult",
            r"subida.*nível.*feedback",
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
        "label": "Positive reaction to audio / sound design",
        "patterns": [
            r"resposta.*auditiva", r"feedback.*auditiv",
            r"som.*ambiental", r"sons.*bem.*escolhido",
            r"audio.*response", r"sound.*design",
            r"boa.*resposta.*auditiva",
        ],
    },
    {
        "label": "Suggestion: more audio feedback / dialogue",
        "patterns": [
            r"mais.*feedback.*auditivo", r"feedback.*auditivo.*errar",
            r"mais.*diálogo", r"more.*dialogue", r"more.*audio",
            r"audio.*feedback.*miss",
        ],
    },

    # ------------------------------------------------------------------
    # IMMERSION & ENVIRONMENT
    # ------------------------------------------------------------------
    {
        "label": "Positive reaction to immersion / environment",
        "patterns": [
            r"imersiv", r"immersi", r"ambiente.*diferente",
            r"cenário.*bem", r"som.*ambiental", r"fair.*3d",
            r"truly.*seeing.*fair", r"feira.*3d",
            r"felt.*i was.*fair", r"felt.*in.*fair",
            r"ambiente.*jogo", r"envolvência",
        ],
    },

    # ------------------------------------------------------------------
    # GAMES – GENERAL POSITIVES
    # ------------------------------------------------------------------
    {
        "label": "Game praised as fun / engaging / intuitive",
        "patterns": [
            r"divertid", r"fun", r"engag", r"intuitiv",
            r"simples.*acessiv", r"fácil.*compreender",
            r"easy.*understand", r"easy.*use",
            r"simplicit", r"vibrant",
            r"genuinely.*having fun", r"really.*fun",
            r"muito.*bem.*concebido",
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
        ],
    },

    # ------------------------------------------------------------------
    # SCORING / GAME MODES
    # ------------------------------------------------------------------
    {
        "label": "Suggestion: highscore / competitive mode",
        "patterns": [
            r"highscore", r"high.*score", r"score.*mode",
            r"acréscimo.*pontos", r"register.*highscore",
            r"limited.*time.*mode", r"time.*mode",
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

    # ------------------------------------------------------------------
    # REHABILITATION CONTEXT
    # ------------------------------------------------------------------
    {
        "label": "Mentioned rehabilitation / therapeutic value",
        "patterns": [
            r"reabilita", r"rehabilit", r"terapêutic", r"therapeut",
            r"stroke", r"acidente.*vascular",
        ],
    },

    # ------------------------------------------------------------------
    # GRAPHICS / VISUALS
    # ------------------------------------------------------------------
    {
        "label": "Negative comment on graphics / visuals",
        "patterns": [
            r"didn.t.*liked.*graphic", r"graphics.*bad",
            r"graphic.*not.*great", r"visual.*poor",
        ],
    },
]

UNCATEGORIZED_LABEL = "Other / uncategorized"


# ---------------------------------------------------------------------------
# CSV LOADER – robust to column-count mismatches
# ---------------------------------------------------------------------------

def load_csv(csv_path: str) -> pd.DataFrame:
    """
    Load the CSV, tolerating rows with more fields than the header declares.

    Uses Python's csv module to read raw rows, then truncates each data row
    to the header column count — preventing any participant from being dropped
    due to unescaped commas in free-text fields.
    """
    raw_text = Path(csv_path).read_text(encoding="utf-8-sig")
    reader = csv.reader(io.StringIO(raw_text))
    rows = list(reader)

    if not rows:
        raise ValueError(f"Empty file: {csv_path}")

    header = [col.strip() for col in rows[0]]
    n_cols = len(header)

    records = []
    for row in rows[1:]:
        if not row or not row[0].strip():
            continue
        padded = (row + [""] * n_cols)[:n_cols]
        records.append({header[i]: padded[i].strip() for i in range(n_cols)})

    return pd.DataFrame(records)


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
    Gather all non-empty free-text responses from a participant row,
    one string per non-empty column.
    """
    responses = []
    for col in text_cols:
        val = row.get(col, "")
        if isinstance(val, str) and val.strip():
            responses.append(val.strip())
    return responses


def match_themes(response: str) -> list[str]:
    """
    Return all theme labels that match the response.
    A response can belong to multiple themes (unlike the observations script
    which assigns only the first match), because suggestions often address
    several topics at once.
    """
    matched = []
    obs_lower = response.lower()
    for theme in THEMES:
        for pattern in theme["patterns"]:
            if re.search(pattern, obs_lower):
                matched.append(theme["label"])
                break  # move to next theme once one pattern hits
    return matched if matched else [UNCATEGORIZED_LABEL]


# ---------------------------------------------------------------------------
# MAIN ANALYSIS
# ---------------------------------------------------------------------------

def analyse(csv_path: str) -> tuple[pd.DataFrame, int]:
    """
    Load the CSV, analyse free-text responses, and return a summary DataFrame.

    Each participant is counted at most once per theme, regardless of how many
    of their responses matched that theme.

    Returns
    -------
    tuple[pd.DataFrame, int]
        Summary table with columns (Theme | Participants | Percentage)
        and the total number of respondents with at least one free-text answer.
    """
    df = load_csv(csv_path)

    text_cols = resolve_text_columns(df.columns.tolist())
    if not text_cols:
        raise RuntimeError(
            "Could not find free-text columns. "
            f"Check TEXT_COLUMN_SUBSTRINGS against headers: {df.columns.tolist()}"
        )

    print(f"\n{'='*62}")
    print(f"  Total rows loaded       : {len(df)}")
    print(f"  Free-text columns found :")
    for col in text_cols:
        print(f"    • {col}")
    print(f"{'='*62}\n")

    # Count participants who have at least one non-empty response
    has_response = df[text_cols].apply(
        lambda col: col.str.strip().ne(""), axis=0
    ).any(axis=1)
    total_with_response = int(has_response.sum())

    # theme_label -> set of participant row indices that triggered it
    theme_to_participants: dict[str, set] = defaultdict(set)

    for idx, row in df.iterrows():
        responses = collect_responses(row, text_cols)
        if not responses:
            continue
        # Aggregate all themes mentioned across all free-text fields
        themes_for_participant: set[str] = set()
        for response in responses:
            for theme_label in match_themes(response):
                themes_for_participant.add(theme_label)

        for theme_label in themes_for_participant:
            theme_to_participants[theme_label].add(idx)

    records = []
    for theme_label, participants in theme_to_participants.items():
        count = len(participants)
        pct = round(count / total_with_response * 100, 1)
        records.append({
            "Theme": theme_label,
            "Participants": count,
            "Percentage (%)": pct,
        })

    summary = pd.DataFrame(records)
    summary.sort_values("Participants", ascending=False, inplace=True)
    summary.reset_index(drop=True, inplace=True)

    return summary, total_with_response


def print_report(summary: pd.DataFrame, total: int) -> None:
    """Pretty-print the full summary table to stdout."""
    print(f"\n{'='*62}")
    print(f"  SUGGESTION / FEEDBACK THEME ANALYSIS")
    print(f"  (out of {total} participants with at least one response)")
    print(f"{'='*62}")
    print(f"{'#':<4} {'Theme':<50} {'n':>4}  {'%':>6}")
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

    print(f"{'='*62}\n")


# ---------------------------------------------------------------------------
# ENTRY POINT
# ---------------------------------------------------------------------------

if __name__ == "__main__":
    summary, total = analyse(FILE_PATH)
    print_report(summary, total)

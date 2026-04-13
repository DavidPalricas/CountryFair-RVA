"""
analyze_suggestions.py
======================
Reads the UserTestResultsWave2 file (.xlsx), extracts open-ended
responses from three free-text columns:
    - "What did you like most and less about the game."
    - "Do you have any suggestions, comments, or thoughts about your
       experience with the personalized XR session?"
    - "Other Suggestions"

Each response is matched against thematic categories via regex patterns.
A single response can belong to multiple themes simultaneously, since
suggestions often address several topics at once. Each participant is
counted at most once per theme.

The report shows how many participants mentioned each theme and the
corresponding percentage of the total respondent pool (participants with
at least one non-empty free-text response).
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
            r"instrução.*fonte", r"fonte.*instrução",
            r"dificuldade.*ler", r"dificuldade a ler",
            r"aproxima.*caixas", r"distância.*caixas",
            r"closer.*text", r"text.*closer",
            r"change.*aspect.*easier.*reading",
            r"easier.*reading",
        ],
    },
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
            r"strange places.*button", r"look.*strange.*places",
            r"button.*press.*one finger", r"press.*button.*one finger",
            r"button.*doesn.t.*work", r"button.*not.*work",
            r"movimentação.*menu principal", r"movement.*main menu",
            r"interagir.*ambiente.*tendas", r"ir de encontro.*tendas",
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
            r"mapping.*physical.*movement.*frisbee",
            r"physical.*movement.*frisbee.*trajectory",
            r"frisbee.*trajectory.*mapping",
            r"movement.*frisbee.*not.*intuitive",
        ],
    },
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
        "label": "Positive reaction to archery game",
        "patterns": [
            r"liked.*bow.*arrow", r"bow.*arrow.*very.*much",
            r"bow.*arrow.*easy", r"bow.*arrow.*fun",
            r"arco.*e.*flecha.*intuitiv", r"arco.*e.*flecha.*divertid",
            r"arco.*e.*flecha.*facil",
            r"like.*bow.*and.*arrow", r"enjoyed.*archery",
            r"bow.*and.*arrow.*pleasing", r"bow.*and.*arrow.*enjoyable",
            r"easy.*fine.*tune.*difficulty",
            r"like.*bow.*and.*arrow.*very.*much",
        ],
    },
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
        "label": "Positive feedback on adaptive difficulty",
        "patterns": [
            r"ajuste.*automático", r"dificuldade.*ajust",
            r"ajust.*dificuldade", r"difficulty.*adjust",
            r"adapt.*difficulty", r"dificuldade.*crescente",
            r"increasing.*difficulty", r"difficulty.*increasing",
            r"dificuldade.*foi aumentando",
            r"areas.*foram.*ajustando", r"areas.*ajustando.*dificuldade",
        ],
    },
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
            r"mais.*diálogo", r"more.*dialogue", r"more.*audio.*feedback",
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
            r"truly.*seeing.*fair",
            r"ambiente.*jogo", r"envolvência",
            r"realistic.*movement", r"realistic.*task",
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
            r"interessante.*mesmo.*ao.*longo",
            r"mantém.*interessante", r"interativo",
            r"conceito.*interessante",
            r"acessível.*dinâmico",
            r"duração.*adequada",
            r"instruções.*faceis.*entender",
            r"jogos.*em.*si",
            r"sistema.*intuitivo.*fácil",
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

    # ------------------------------------------------------------------
    # REHABILITATION CONTEXT
    # ------------------------------------------------------------------
    {
        "label": "Mentioned rehabilitation / therapeutic value",
        "patterns": [
            r"reabilita", r"rehabilit", r"terapêutic", r"therapeut",
            r"stroke", r"acidente.*vascular",
            r"útil.*reabilita", r"reabilita.*divertid",
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
            r"didnt.*liked.*graphic",
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

    for idx, row in df.iterrows():
        responses = collect_responses(row, text_cols)
        if not responses:
            continue
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
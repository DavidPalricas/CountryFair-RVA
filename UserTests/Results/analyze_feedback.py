"""
analyze_feedback.py
===================
Reads the UserTestResultsWave2 file (.xlsx or .csv), analyses free-text
responses from question 20:
    - Q20: "What did you like most and less about the game."

Each response is matched against thematic feedback categories defined here.
The summary table is followed by a breakdown showing each response and the
category it was assigned to.
"""

import sys
from collections import defaultdict
from pathlib import Path

sys.path.insert(0, str(Path(__file__).parent))
from analyze_suggestions import (  # noqa: E402
    FILE_PATH,
    load_file,
)

Q20_SUBSTRING = "like most"
NON_ANSWERS = {"no", "nop", "no.", "none", "-", "n/a", "", "nan"}
W = 62
UNCATEGORIZED_LABEL = "Other / uncategorized"

# ---------------------------------------------------------------------------
# FEEDBACK THEME DEFINITIONS (separate from suggestions)
# ---------------------------------------------------------------------------
FEEDBACK_THEMES: list[dict] = [

    # ------------------------------------------------------------------
    # GENERAL POSITIVE
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
            r"jogos.*em.*si", r"jogos.*em.*s\u00ed",
            r"sistema.*intuitivo.*fácil",
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
    # ARCHERY
    # ------------------------------------------------------------------
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

    # ------------------------------------------------------------------
    # FRISBEE
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

    # ------------------------------------------------------------------
    # TEXT / UI
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

    # ------------------------------------------------------------------
    # AUDIO
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

    # ------------------------------------------------------------------
    # MENU / BUTTONS
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
    # GRAPHICS
    # ------------------------------------------------------------------
    {
        "label": "Negative comment on graphics / visuals",
        "patterns": [
            r"didn.t.*liked.*graphic", r"graphics.*bad",
            r"graphic.*not.*great", r"visual.*poor",
            r"didnt.*liked.*graphic",
        ],
    },

    # ------------------------------------------------------------------
    # REHABILITATION
    # ------------------------------------------------------------------
    {
        "label": "Mentioned rehabilitation / therapeutic value",
        "patterns": [
            r"reabilita", r"rehabilit", r"terapêutic", r"therapeut",
            r"stroke", r"acidente.*vascular",
            r"útil.*reabilita", r"reabilita.*divertid",
        ],
    },
]


# ---------------------------------------------------------------------------
# THEME MATCHING
# ---------------------------------------------------------------------------

import re

def match_feedback_themes(response: str) -> list[str]:
    matched = []
    obs_lower = response.lower()
    for theme in FEEDBACK_THEMES:
        for pattern in theme["patterns"]:
            if re.search(pattern, obs_lower):
                matched.append(theme["label"])
                break
    return matched if matched else [UNCATEGORIZED_LABEL]


# ---------------------------------------------------------------------------
# ANALYSIS
# ---------------------------------------------------------------------------

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

        themes = match_feedback_themes(val)
        if themes == [UNCATEGORIZED_LABEL]:
            continue

        for t in themes:
            theme_to_participants[t].add(idx)
            theme_to_responses[t].append((idx, val))

    records = []
    for theme_label, participants in theme_to_participants.items():
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


# ---------------------------------------------------------------------------
# DISPLAY
# ---------------------------------------------------------------------------

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

    print(f"\n{'─'*W}")
    print(f"  Respostas por categoria")
    print(f"{'─'*W}")

    for row in records:
        print(f"\n  >> {row['Theme']}")
        for participant, response in row["Responses"]:
            print(f"\n     Participante #{participant}")
            _wrap_print(response)

    print(f"\n{'='*W}\n")


# ---------------------------------------------------------------------------
# ENTRY POINT
# ---------------------------------------------------------------------------

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
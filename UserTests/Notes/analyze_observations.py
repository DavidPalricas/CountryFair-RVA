"""
analyze_observations.py
=======================
Reads the UserTestWave2.csv file, extracts individual observations from the
'General_Observations' column, clusters them into thematic categories using
keyword matching, and reports how many participants triggered each category
along with the percentage of the total participant pool.
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
FILE_PATH = "UserTestWave2.csv"


# ---------------------------------------------------------------------------
# THEME DEFINITIONS
# Each theme has a label and a list of keyword/phrase regex patterns.
# A single observation line is assigned to the first theme whose pattern
# matches. Order matters: put more-specific themes before broader ones.
# ---------------------------------------------------------------------------
THEMES: list[dict] = [

    # ------------------------------------------------------------------
    # ENGAGEMENT / MOTIVATION
    # ------------------------------------------------------------------
    {
        "label": "Wanted to keep playing after the test ended",
        "patterns": [r"keep playing", r"after the test"],
    },

    # ------------------------------------------------------------------
    # MINI-GAME PREFERENCE
    # ------------------------------------------------------------------
    {
        "label": "Preferred / favourite: frisbee",
        "patterns": [
            r"favorite.*frisbee", r"favour.*frisbee",
            r"frisbee.*favorite", r"frisbee.*favour",
            r"only wanted to play.*frisbee",
            r"preferred.*frisbee",
        ],
    },
    {
        "label": "Preferred / favourite: archery",
        "patterns": [
            r"favorite.*archery", r"favour.*archery",
            r"archery.*favorite", r"archery.*favour",
            r"favorite.*mini.game.*archery", r"archery.*favorite.*mini.game",
            r"preferred.*archery",
        ],
    },
    {
        "label": "Preferred / favourite: both or no clear preference",
        "patterns": [
            r"liked both", r"preferred.*both", r"both mini.game",
            r"liked both mini.game",
        ],
    },

    # ------------------------------------------------------------------
    # USABILITY – INTERACTION / MENU
    # ------------------------------------------------------------------
    {
        "label": "Difficulty with eye-gaze / button / menu interaction",
        "patterns": [
            r"eye.gaze", r"gaze interaction", r"clunky.*select",
            r"look precisely", r"press.*button", r"pressing.*button",
            r"not.*obvious.*look.*tent", r"look at the tent",
            r"frisbee button.*positioned", r"difficul.*press.*button",
            r"difficulty.*press.*button",
            r"did not like.*having.*look.*tent",
            r"look at the tents.*button",
        ],
    },

    # ------------------------------------------------------------------
    # USABILITY – FRISBEE
    # ------------------------------------------------------------------
    {
        "label": "Difficulty with frisbee mechanics (picking up / releasing / throwing)",
        "patterns": [
            r"trouble.*pick.*frisbee", r"difficul.*releas.*frisbee",
            r"difficul.*frisbee", r"how to pick up the frisbee",
            r"pick up the frisbee", r"difficul.*throw.*frisbee",
            r"had difficulties.*frisbee", r"throwing.*frisbee.*beginning",
            r"control.*throwing force", r"frisbee.*more complicated",
            r"struggled.*frisbee", r"frisbee.*difficult",
            r"difficulties.*frisbee",
            r"didn.t understand.*frisbee.*throw",
            r"lot of difficulties.*frisbee",
            r"observer.*demonstrate.*grab.*frisbee",
            r"had to intervene.*frisbee",
        ],
    },
    {
        "label": "Negative reaction to frisbee game (boring / unengaging)",
        "patterns": [
            r"frisbee.*boring", r"frisbee.*boring",
            r"frisbee.*secante", r"frisbee.*not.*motivat",
            r"didn.t motivat.*frisbee",
        ],
    },
    {
        "label": "Confusion about frisbee scoring (dog area vs extras)",
        "patterns": [
            r"dog.*worth more", r"area around the dog",
            r"thought it was the opposite", r"extra areas",
            r"only.*throw.*dog", r"focused.*throw.*dog",
            r"score.*more.*point", r"area.*worth",
        ],
    },
    {
        "label": "Questioned frisbee trajectory / physics",
        "patterns": [
            r"frisbee.*trajectory.*curved", r"trajectory.*curved",
            r"curved.*trajectory", r"questioned.*frisbee.*trajectory",
            r"frisbee.*physics.*weird", r"frisbee.*physics",
        ],
    },

    # ------------------------------------------------------------------
    # USABILITY – ARCHERY
    # ------------------------------------------------------------------
    {
        "label": "Difficulty with bow / archery mechanics",
        "patterns": [
            r"difficul.*bow", r"how to pull.*bow", r"pull.*string",
            r"pull.*bow", r"difficul.*archery",
            r"arrow.*release.*too early", r"difficulty.*pull.*bowstring",
            r"had difficulty.*pull.*bowstring",
            r"bow.*too sensitive", r"bow.*very sensitive",
            r"arrow.*release.*when.*pulled", r"too sensitive.*bow",
            r"hand.*disappear.*archery", r"hand.*disappeared.*pull",
        ],
    },

    # ------------------------------------------------------------------
    # USABILITY – TEXT / READABILITY
    # ------------------------------------------------------------------
    {
        "label": "Difficulty reading text (UI readability)",
        "patterns": [
            r"text.*difficult.*read", r"difficult.*read.*text",
            r"found.*text.*difficult", r"text.*hard.*read",
            r"not.*able.*read.*text", r"complain.*read.*text",
            r"letters.*blurry", r"blurry.*letter",
            r"spelling.*mistake", r"spelling.*error",
            r"did not notice.*text.*dialogue",
        ],
    },
    {
        "label": "Did not read / ignored instructions",
        "patterns": [
            r"didn.t.*read.*instruct", r"didn.t care.*instruct",
            r"didn.t care about.*mini.game.*instruct",
            r"observer.*explain.*rules",
            r"could not remember.*rules",
        ],
    },

    # ------------------------------------------------------------------
    # ADAPTIVE SYSTEM AWARENESS
    # ------------------------------------------------------------------
    {
        "label": "Did not notice adaptive / color changes in archery",
        "patterns": [
            r"did not notice.*color", r"didn.t.*realiz.*customiz",
            r"didn.t.*notice", r"not notice.*color",
            r"did not notice.*balloon", r"did not notice.*adaptive",
            r"did not notice.*specific color",
            r"did not notice.*had to hit.*color",
            r"did not notice.*had to pop",
            r"did not notice.*hit a specific color",
            r"had to hit a specific color",
            r"color.*not obvious", r"change.*color.*not obvious",
            r"did not notice.*text.*dialogue.*changing",
        ],
    },
    {
        "label": "Noticed adaptive changes (balloons / difficulty / dog distance)",
        "patterns": [
            r"noticed.*adaptive", r"adaptive.*changed",
            r"balloons started moving", r"attentive.*color.*changed",
            r"very attentive.*color",
            r"noticed.*dog.*farther", r"dog.*farther.*difficulty",
            r"dog went farther", r"dog.*distance.*increased",
        ],
    },
    {
        "label": "Curiosity about adaptive difficulty / rehabilitation",
        "patterns": [
            r"curiosit.*difficulty", r"how.*difficulty.*adjust",
            r"linked.*customiz", r"rehabilitation",
            r"scientific reasoning", r"stroke.*rehabilitation",
            r"beneficial.*stroke",
        ],
    },

    # ------------------------------------------------------------------
    # TECHNICAL ISSUES
    # ------------------------------------------------------------------
    {
        "label": "Technical issue / bug / crash",
        "patterns": [
            r"bug", r"crash", r"fell.*infinitely", r"fell off the map",
            r"poor calibration", r"headset.*falling",
            r"hand tracking.*not working", r"room.*not.*lit", r"poor setup",
            r"color.*changed randomly", r"arrow.*didn.t.*return",
            r"frisbee.*crooked", r"arrow.*fell.*map", r"shot.*off.*map",
            r"hands.*too far behind.*sensor",
            r"hands ended up too far behind",
            r"color.*alternate.*miss", r"color.*should.*not.*change",
            r"poorly lit", r"lighting.*space",
            r"spelling.*mistake", r"spelling.*error",
        ],
    },

    # ------------------------------------------------------------------
    # EMOTIONAL REACTIONS – NEGATIVE
    # ------------------------------------------------------------------
    {
        "label": "Frustration during gameplay",
        "patterns": [
            r"frustrat", r"tilted", r"cursing", r"getting tilted",
            r"throw.*sometimes worked.*sometimes didn.t",
            r"cursed.*archery", r"cursed.*button",
            r"getting.*frustrated", r"showed frustration",
        ],
    },

    # ------------------------------------------------------------------
    # EMOTIONAL REACTIONS – POSITIVE
    # ------------------------------------------------------------------
    {
        "label": "Positive reaction to frisbee game",
        "patterns": [
            r"frisbee.*fun", r"loved.*frisbee", r"liked.*frisbee",
            r"frisbee.*liked", r"frisbee.*cool", r"frisbee.*cute",
            r"fun.*frisbee", r"having fun.*frisbee",
            r"laughed.*frisbee", r"frisbee.*laugh",
            r"throwing.*frisbee.*cool",
            r"liked.*mechanic.*grab.*throw.*frisbee",
            r"mechanic.*grabbing.*throwing.*frisbee",
            r"fun.*both.*games", r"fun in both games",
            r"cheering.*scoring",
        ],
    },
    {
        "label": "Positive reaction to archery game",
        "patterns": [
            r"archery.*cool", r"archery.*great", r"archery.*awesome",
            r"loved.*archery", r"liked.*archery", r"archery.*liked",
            r"archery.*very cool",
            r"archery.*controlled well",
            r"archery.*very good", r"found.*bow.*funny",
            r"bow.*amusing",
        ],
    },
    {
        "label": "Positive reaction to overall experience / environment",
        "patterns": [
            r"loved.*environment", r"liked.*atmosphere", r"fair.*fun",
            r"fair.*realistic", r"country fair", r"environment.*fun",
            r"experience.*cool", r"experience.*fun",
            r"experience.*immersive", r"experience.*interactive",
            r"loved.*overall.*experience", r"loved.*experience",
            r"vr.*experience.*interesting", r"vr.*experiences.*interesting",
            r"experience.*very interesting",
            r"liked.*experience.*saying.*good",
            r"enjoyed.*exploring.*environment",
            r"exploring.*environment.*turning.*head",
        ],
    },
    {
        "label": "Did not understand the objective / rules of the game",
        "patterns": [
            r"did not understand.*objective", r"didn.t understand.*objective",
            r"could not remember.*rules", r"did not understand.*archery",
            r"wanted to switch.*mini.game", r"wanted.*switch.*game",
            r"halfway.*wanted.*switch",
        ],
    },

    # ------------------------------------------------------------------
    # WORLD / IMMERSION
    # ------------------------------------------------------------------
    {
        "label": "Engagement with NPCs / animals / balloons",
        "patterns": [
            r"npc", r"follow.*npc", r"balloon.*fly", r"balloon.*pop",
            r"watching.*npc", r"animals.*present", r"people walking",
            r"characters walking", r"animations.*character",
            r"liked.*npc", r"npcs.*background",
            r"liked.*deer", r"really liked.*deer",
            r"liked.*grass",
        ],
    },
    {
        "label": "Liked the dog / frisbee dog mechanic",
        "patterns": [
            r"dog.*cute", r"dog.*catching", r"dog.*fetching",
            r"dog.*returning", r"liked.*dog", r"mechanic.*dog",
            r"dog.*deliver", r"dog.*brings.*frisbee.*back",
            r"dog.*barking", r"spatial.*sound.*dog",
            r"locate.*dog.*sound", r"loved.*dog",
            r"dog.*model", r"talking.*dog", r"called.*dog",
            r"kept.*talking.*dog", r"bigodes", r"whiskers",
        ],
    },
    {
        "label": "Positive reaction to hand tracking precision",
        "patterns": [
            r"hand tracking.*precise", r"hand tracking.*fast",
            r"impressed.*hand tracking",
            r"hand tracking.*very precise",
        ],
    },
    {
        "label": "Positive reaction to hands / hand tracking novelty",
        "patterns": [
            r"liked.*hands", r"really liked.*hands",
            r"inappropriate gesture", r"making.*gesture",
        ],
    },

    # ------------------------------------------------------------------
    # BEHAVIOUR
    # ------------------------------------------------------------------
    {
        "label": "Distraction / messing around / off-task behaviour",
        "patterns": [
            r"messing around", r"avoiding.*objective", r"throw.*deer",
            r"throw.*extra", r"skill issue", r"distraction",
            r"exploit", r"rolled.*towards.*target",
        ],
    },

    # ------------------------------------------------------------------
    # INSTRUCTION ENGAGEMENT
    # ------------------------------------------------------------------
    {
        "label": "Read instructions carefully",
        "patterns": [
            r"read.*instruction", r"carefully.*instruction",
            r"instruction.*carefully", r"read.*out loud",
            r"attentive.*reading", r"attentive.*instruction",
        ],
    },
    {
        "label": "Tutorial / demo video engagement",
        "patterns": [
            r"tutorial.*video", r"video.*tutorial", r"video.*cool",
            r"demonstration video", r"demo.*video",
            r"didn.t.*want.*tutorial", r"skipped.*tutorial",
            r"focused.*watching.*demonstration",
            r"very focused.*watching.*demonstration",
        ],
    },

    # ------------------------------------------------------------------
    # PHYSICAL / HEALTH
    # ------------------------------------------------------------------
    {
        "label": "Physical discomfort (dizziness, fatigue, pain)",
        "patterns": [
            r"felt dizzy", r"diz+y", r"arm.*tired",
            r"muscle pain", r"fatigue",
            r"arm.*hurt", r"arm hurt",
        ],
    },

    # ------------------------------------------------------------------
    # LEFT-HANDEDNESS
    # ------------------------------------------------------------------
    {
        "label": "Left-handed participant reported difficulty",
        "patterns": [
            r"left.hand", r"left.*handed",
            r"pull.*bowstring.*left.*hand",
            r"throw.*frisbee.*left.*hand",
            r"left hand.*more difficult",
        ],
    },

    # ------------------------------------------------------------------
    # CONTENT / WORDING FEEDBACK
    # ------------------------------------------------------------------
    {
        "label": "Feedback on game wording / terminology",
        "patterns": [
            r"avoid.*term.*impossible", r"impossible.*more difficult",
            r"term.*impossible", r"wording.*impossible",
        ],
    },

    # ------------------------------------------------------------------
    # SUGGESTIONS
    # ------------------------------------------------------------------
    {
        "label": "Suggestion / unprompted feedback",
        "patterns": [
            r"suggest", r"highscore", r"trajectory color",
            r"something more interactive", r"arrow.*saying.*look here",
            r"not.*obvious.*look",
            r"visual aid.*frisbee", r"frisbee.*visual aid",
        ],
    },
]

UNCATEGORIZED_LABEL = "Other / uncategorized"


# ---------------------------------------------------------------------------
# CSV LOADER – robust to column-count mismatches (e.g. User 22)
# ---------------------------------------------------------------------------

def load_csv(csv_path: str) -> pd.DataFrame:
    """
    Load the CSV robustly, tolerating rows with more fields than the header.

    The file has two header rows:
        Row 0 – generic Column1..Column6 (skip)
        Row 1 – real column names

    Some rows (e.g. User 22) have an unescaped comma in the last column,
    producing 7 fields. Standard parsers drop these rows. We handle them by
    reading raw text with Python's csv module, keeping only the first
    len(header) fields per data row.
    """
    raw_text = Path(csv_path).read_text(encoding="utf-8-sig")
    reader = csv.reader(io.StringIO(raw_text))
    rows = list(reader)

    # Row index 1 is the real header (row 0 is the dummy Column1..N header)
    header = [col.strip() for col in rows[1]]
    n_cols = len(header)

    records = []
    for row in rows[2:]:
        if not row or not row[0].strip():
            continue  # skip empty / trailing rows
        # Truncate to header width – absorbs extra comma-split fields
        padded = (row + [""] * n_cols)[:n_cols]
        records.append({header[i]: padded[i].strip() for i in range(n_cols)})

    return pd.DataFrame(records)


# ---------------------------------------------------------------------------
# HELPERS
# ---------------------------------------------------------------------------

def parse_observations(raw: str) -> list[str]:
    """
    Split a raw observation cell into individual numbered items.

    Observations are formatted as:
        "1. Text one\\n2. Text two\\n3. Text three"

    Returns a list of cleaned observation strings (without the leading number).
    """
    if not isinstance(raw, str) or not raw.strip():
        return []
    items = re.split(r"\d+\.\s+", raw)
    return [item.strip() for item in items if item.strip()]


def match_theme(observation: str) -> str:
    """
    Return the label of the first theme whose patterns match the observation.
    Falls back to UNCATEGORIZED_LABEL if nothing matches.
    Matching is case-insensitive.
    """
    obs_lower = observation.lower()
    for theme in THEMES:
        for pattern in theme["patterns"]:
            if re.search(pattern, obs_lower):
                return theme["label"]
    return UNCATEGORIZED_LABEL


# ---------------------------------------------------------------------------
# MAIN ANALYSIS
# ---------------------------------------------------------------------------

def analyse(csv_path: str) -> tuple[pd.DataFrame, int]:
    """
    Load the CSV, analyse observations, and return a summary DataFrame.

    Parameters
    ----------
    csv_path : str
        Path to the CSV file.

    Returns
    -------
    tuple[pd.DataFrame, int]
        Summary table with columns (Theme | Participants | Percentage)
        and the total number of participants.
    """
    df = load_csv(csv_path)

    total_participants = len(df)
    print(f"\n{'='*62}")
    print(f"  Total participants found: {total_participants}")
    print(f"{'='*62}\n")

    # Map theme_label -> set of participant IDs that triggered it
    theme_to_users: dict[str, set] = defaultdict(set)
    theme_to_responses: dict[str, list[tuple[str, str]]] = defaultdict(list)

    for _, row in df.iterrows():
        participant = row["Participant_ID"].strip()
        observations = parse_observations(row.get("General_Observations", ""))

        for obs in observations:
            theme = match_theme(obs)
            theme_to_users[theme].add(participant)
            theme_to_responses[theme].append((participant, obs))

    records = []
    for theme_label, users in theme_to_users.items():
        count = len(users)
        pct = round(count / total_participants * 100, 1)
        records.append({
            "Theme": theme_label,
            "Participants": count,
            "Percentage (%)": pct,
            "Responses": theme_to_responses[theme_label],
        })

    summary = pd.DataFrame(records)
    summary.sort_values("Participants", ascending=False, inplace=True)
    summary.reset_index(drop=True, inplace=True)

    return summary, total_participants


def _wrap_print(text: str, indent: str = "    ", width: int = 70) -> None:
    """Word-wrap text to width, printing with indent."""
    words = text.split()
    line = indent
    for word in words:
        if len(line) + len(word) + 1 > width:
            print(line)
            line = indent + word
        else:
            line += (" " if line.strip() else "") + word
    if line.strip():
        print(line)


def print_report(summary: pd.DataFrame, total: int) -> None:
    """Pretty-print the summary table followed by per-theme response breakdown."""
    W = 62
    print(f"\n{'='*W}")
    print(f"  OBSERVATION THEME ANALYSIS  (out of {total} participants)")
    print(f"{'='*W}")
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

    # ── Per-theme response breakdown ────────────────────────────────────────
    print(f"\n{'─'*W}")
    print(f"  Observações por categoria")
    print(f"{'─'*W}")

    for _, row in summary.iterrows():
        print(f"\n  >> {row['Theme']}")
        for participant, observation in row["Responses"]:
            print(f"\n     Participante {participant}")
            _wrap_print(observation)

    print(f"\n{'='*W}\n")


# ---------------------------------------------------------------------------
# ENTRY POINT
# ---------------------------------------------------------------------------

if __name__ == "__main__":
    summary, total = analyse(FILE_PATH)
    print_report(summary, total)
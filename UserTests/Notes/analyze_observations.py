"""
analyze_observations.py
=======================
Reads the UserTestWave2.csv file, extracts individual observations from the
'General_Observations' column, clusters them into thematic categories using
keyword matching, and reports how many participants triggered each category
along with the percentage of the total participant pool.

Usage:
    python analyze_observations.py

Dependencies: pandas (pip install pandas)
"""

import re
from collections import defaultdict

import pandas as pd


# ---------------------------------------------------------------------------
# FILE PATH CONSTANT
# Change this to point to your CSV file.
# ---------------------------------------------------------------------------
FILE_PATH = "UserTestWave2.csv"


# ---------------------------------------------------------------------------
# THEME DEFINITIONS
# Each theme has a label and a list of keyword/phrase regex patterns.
# A single observation line is assigned to the first theme whose pattern matches.
# Order matters: put more-specific themes before broader/catch-all ones.
# ---------------------------------------------------------------------------
THEMES: list[dict] = [
    {
        "label": "Wanted to keep playing after the test ended",
        "patterns": [r"keep playing", r"after the test"],
    },
    {
        "label": "Difficulty with eye-gaze / button / menu interaction",
        "patterns": [
            r"eye.gaze", r"gaze interaction", r"clunky.*select",
            r"look precisely", r"press.*button", r"pressing.*button",
            r"not.*obvious.*look.*tent", r"look at the tent",
            r"frisbee button.*positioned",
        ],
    },
    {
        "label": "Difficulty with frisbee mechanics (picking up / releasing / throwing)",
        "patterns": [
            r"trouble.*pick.*frisbee", r"difficul.*releas.*frisbee",
            r"difficul.*frisbee", r"how to pick up the frisbee",
            r"pick up the frisbee", r"difficul.*throw.*frisbee",
            r"had difficulties.*frisbee", r"throwing.*frisbee.*beginning",
            r"control.*throwing force", r"frisbee.*more complicated",
            r"struggled.*frisbee",
        ],
    },
    {
        "label": "Confusion about frisbee scoring (dog area vs extras)",
        "patterns": [
            r"dog.*worth more", r"area around the dog", r"thought it was the opposite",
            r"extra areas", r"only.*throw.*dog", r"focused.*throw.*dog",
            r"score.*more.*point", r"area.*worth",
        ],
    },
    {
        "label": "Difficulty with bow / archery mechanics",
        "patterns": [
            r"difficul.*bow", r"how to pull.*bow", r"pull.*string",
            r"pull.*bow", r"difficul.*archery",
            r"arrow.*release.*too early", r"arrow.*fell.*map",
            r"shot.*off.*map",
        ],
    },
    {
        "label": "Did not notice adaptive / color changes in archery",
        "patterns": [
            r"did not notice.*color", r"didn.t.*realiz.*customiz",
            r"didn.t.*notice", r"not notice.*color",
            r"did not notice.*balloon", r"did not notice.*adaptive",
            r"did not notice.*archery.*color",
            r"did not notice.*specific color",
            r"did not notice.*had to hit.*color",
            r"did not notice.*had to pop",
        ],
    },
    {
        "label": "Technical issue / bug / crash",
        "patterns": [
            r"bug", r"crash", r"fell.*infinitely", r"fell off the map",
            r"poor calibration", r"headset.*falling", r"hand tracking.*not working",
            r"room.*not.*lit", r"poor setup",
            r"color.*changed randomly", r"arrow.*didn.t.*return",
            r"frisbee.*crooked",
        ],
    },
    {
        "label": "Frustration during gameplay",
        "patterns": [
            r"frustrat", r"tilted", r"cursing", r"getting tilted",
            r"throw.*sometimes worked.*sometimes didn.t",
        ],
    },
    {
        "label": "Positive reaction to frisbee game",
        "patterns": [
            r"frisbee.*fun", r"frisbee.*favor", r"loved.*frisbee",
            r"liked.*frisbee", r"frisbee.*liked", r"frisbee.*cool",
            r"frisbee.*cute", r"favorite.*frisbee", r"frisbee.*favorite",
            r"fun.*frisbee", r"having fun.*frisbee",
        ],
    },
    {
        "label": "Positive reaction to archery game",
        "patterns": [
            r"archery.*cool", r"archery.*favor", r"favorite.*archery",
            r"archery.*favorite", r"liked.*archery", r"archery.*liked",
            r"archery.*awesome", r"loved.*archery", r"preferred.*archery",
        ],
    },
    {
        "label": "Positive reaction to overall experience / environment",
        "patterns": [
            r"loved.*environment", r"liked.*atmosphere", r"fair.*fun",
            r"fair.*realistic", r"country fair", r"environment.*fun",
            r"experience.*cool", r"experience.*fun", r"experience.*immersive",
            r"experience.*interactive", r"cool.*experience",
        ],
    },
    {
        "label": "Engagement with NPCs / animals / balloons",
        "patterns": [
            r"npc", r"follow.*npc", r"balloon.*fly", r"balloon.*pop",
            r"watching.*npc", r"animals.*present", r"people walking",
            r"characters walking", r"animations.*character",
            r"liked.*npc", r"npcs.*background",
        ],
    },
    {
        "label": "Positive reaction to hands / hand tracking",
        "patterns": [
            r"liked.*hands", r"really liked.*hands", r"inappropriate gesture",
            r"making.*gesture",
        ],
    },
    {
        "label": "Distraction / messing around / off-task behaviour",
        "patterns": [
            r"messing around", r"avoiding.*objective", r"throw.*deer",
            r"throw.*extra", r"skill issue", r"distraction",
            r"exploit", r"rolled.*towards.*target",
        ],
    },
    {
        "label": "Read instructions carefully",
        "patterns": [
            r"read.*instruction", r"carefully.*instruction",
            r"instruction.*carefully", r"read.*out loud",
            r"attentive.*reading", r"attentive.*instruction",
        ],
    },
    {
        "label": "Difficulty reading text (UI readability)",
        "patterns": [
            r"text.*difficult.*read", r"difficult.*read.*text",
            r"found.*text.*difficult",
        ],
    },
    {
        "label": "Physical discomfort (dizziness, fatigue, pain)",
        "patterns": [
            r"felt dizzy", r"diz+y", r"arm.*tired", r"muscle pain",
            r"fatigue",
        ],
    },
    {
        "label": "Curiosity about adaptive difficulty / rehabilitation",
        "patterns": [
            r"curiosit.*difficulty", r"how.*difficulty.*adjust",
            r"adaptive.*changed", r"linked.*customiz",
            r"rehabilitation", r"scientific reasoning",
        ],
    },
    {
        "label": "Tutorial / demo video engagement",
        "patterns": [
            r"tutorial.*video", r"video.*tutorial", r"video.*cool",
            r"demonstration video", r"demo.*video",
            r"didn.t.*want.*tutorial", r"skipped.*tutorial",
        ],
    },
    {
        "label": "Liked the dog / frisbee dog mechanic",
        "patterns": [
            r"dog.*cute", r"dog.*catching", r"dog.*fetching",
            r"dog.*returning", r"liked.*dog", r"mechanic.*dog",
            r"dog.*deliver",
        ],
    },
    {
        "label": "Suggestion / unprompted feedback",
        "patterns": [
            r"suggest", r"highscore", r"trajectory color",
            r"positioned.*low", r"positioned.*high",
            r"something more interactive", r"arrow.*saying.*look here",
            r"not.*obvious.*look",
        ],
    },
]

UNCATEGORIZED_LABEL = "Other / uncategorized"


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

    # Split on numbered markers like "1.", "2.", "3." at the start of a segment
    items = re.split(r"\d+\.\s+", raw)
    # The first element is always empty (text before "1.") – drop it
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
    # Row 0 is a duplicate header row; skip it and use row 1 as the real header
    df_raw = pd.read_csv(csv_path, header=1, encoding="utf-8-sig", on_bad_lines="skip")
    df_raw.columns = df_raw.columns.str.strip()

    # Keep only rows that represent actual participants (non-empty Participant_ID)
    df = df_raw[
        df_raw["Participant_ID"].notna() &
        df_raw["Participant_ID"].str.strip().ne("")
    ].copy()
    df.reset_index(drop=True, inplace=True)

    total_participants = len(df)
    print(f"\n{'='*62}")
    print(f"  Total participants found: {total_participants}")
    print(f"{'='*62}\n")

    # Map theme_label -> set of participant IDs that triggered it
    theme_to_users: dict[str, set] = defaultdict(set)

    for _, row in df.iterrows():
        participant = row["Participant_ID"].strip()
        observations = parse_observations(row.get("General_Observations", ""))

        for obs in observations:
            theme = match_theme(obs)
            theme_to_users[theme].add(participant)

    # Build summary DataFrame
    records = []
    for theme_label, users in theme_to_users.items():
        count = len(users)
        pct = round(count / total_participants * 100, 1)
        records.append({
            "Theme": theme_label,
            "Participants": count,
            "Percentage (%)": pct,
        })

    summary = pd.DataFrame(records)
    summary.sort_values("Participants", ascending=False, inplace=True)
    summary.reset_index(drop=True, inplace=True)

    return summary, total_participants


def print_report(summary: pd.DataFrame, total: int) -> None:
    """Pretty-print the full summary table to stdout."""
    print(f"\n{'='*62}")
    print(f"  OBSERVATION THEME ANALYSIS  (out of {total} participants)")
    print(f"{'='*62}")
    print(f"{'#':<4} {'Theme':<50} {'n':>4}  {'%':>6}")
    print(f"{'-'*4} {'-'*50} {'-'*4}  {'-'*6}")

    for i, row in summary.iterrows():
        # Wrap long theme names at 50 characters
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
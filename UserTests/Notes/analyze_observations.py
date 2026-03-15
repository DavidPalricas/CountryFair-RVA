"""
analyze_observations.py
=======================
Reads the UserTestWave2.csv file, extracts individual observations from the
'General_Observations' column, clusters them into thematic categories using
keyword matching, and reports how many participants triggered each category
along with the percentage of the total participant pool.

Usage:
    python analyze_observations.py
    python analyze_observations.py --csv path/to/file.csv
    python analyze_observations.py --csv path/to/file.csv --top 10

Dependencies: pandas (pip install pandas)
"""

import re
import argparse
from collections import defaultdict

import pandas as pd

# ---------------------------------------------------------------------------
# FILE PATH CONSTANT
# Change this to point to your CSV file.
# ---------------------------------------------------------------------------
FILE_PATH = "UserTestWave2.csv"


# ---------------------------------------------------------------------------
# 1.  THEME DEFINITIONS
#     Each theme has a label and a list of keyword/phrase patterns.
#     A single observation line is assigned to a theme if ANY pattern matches.
#     Order matters: the first matching theme wins, so put more-specific themes
#     first and the catch-all last.
# ---------------------------------------------------------------------------
THEMES: list[dict] = [
    {
        "label": "Wanted to keep playing after the test ended",
        "patterns": [r"keep playing", r"after the test"],
    },
    {
        "label": "Difficulty with eye-gaze / button interaction",
        "patterns": [r"eye.gaze", r"gaze interaction", r"clunky.*select", r"look precisely", r"press.*button", r"pressing.*button"],
    },
    {
        "label": "Difficulty with frisbee mechanics (picking up / releasing)",
        "patterns": [r"trouble.*pick.*frisbee", r"difficul.*releas.*frisbee", r"difficul.*frisbee",
                     r"how to pick up the frisbee", r"pick up the frisbee"],
    },
    {
        "label": "Confusion about frisbee scoring (dog area vs extras)",
        "patterns": [r"dog.*worth more", r"area around the dog", r"thought it was the opposite",
                     r"extra areas", r"only.*throw.*dog", r"focused.*throw.*dog",
                     r"score.*more.*point", r"area.*worth"],
    },
    {
        "label": "Difficulty with bow / archery mechanics",
        "patterns": [r"difficul.*bow", r"how to pull.*bow", r"pull.*string", r"pull.*bow",
                     r"difficul.*archery"],
    },
    {
        "label": "Did not notice adaptive / color changes",
        "patterns": [r"did not notice.*color", r"didn.t.*realiz.*customiz",
                     r"didn.t.*notice", r"not notice.*color", r"did not notice.*balloon",
                     r"did not notice.*adaptive"],
    },
    {
        "label": "Positive reaction to frisbee game",
        "patterns": [r"frisbee.*fun", r"frisbee.*favor", r"loved.*frisbee",
                     r"liked.*frisbee", r"frisbee.*liked", r"frisbee.*cool",
                     r"frisbee.*cute", r"favorite.*frisbee", r"frisbee.*favorite"],
    },
    {
        "label": "Positive reaction to archery game",
        "patterns": [r"archery.*cool", r"archery.*favor", r"favorite.*archery",
                     r"archery.*favorite", r"liked.*archery", r"archery.*liked"],
    },
    {
        "label": "Positive reaction to environment / atmosphere",
        "patterns": [r"loved.*environment", r"liked.*atmosphere", r"fair.*fun",
                     r"fair.*realistic", r"country fair", r"environment.*fun",
                     r"liked.*atmosphere", r"atmosphere.*cute"],
    },
    {
        "label": "Engagement with NPCs / animals / balloons",
        "patterns": [r"npc", r"follow.*npc", r"balloon.*fly", r"balloon.*pop",
                     r"watching.*npc", r"animals.*present", r"people walking",
                     r"characters walking", r"animations.*character"],
    },
    {
        "label": "Distraction / messing around / off-task behaviour",
        "patterns": [r"messing around", r"avoiding.*objective", r"throw.*deer",
                     r"throw.*extra", r"skill issue", r"distraction"],
    },
    {
        "label": "Technical issue / bug / crash",
        "patterns": [r"bug", r"crash", r"fell.*infinitely", r"fell off the map",
                     r"poor calibration", r"headset.*falling", r"hand tracking.*not working",
                     r"room.*not.*lit", r"poor setup"],
    },
    {
        "label": "Physical discomfort (dizziness, fatigue, pain)",
        "patterns": [r"felt dizzy", r"diz+y", r"arm.*tired", r"muscle pain",
                     r"pain", r"fatigue"],
    },
    {
        "label": "Read instructions carefully",
        "patterns": [r"read.*instruction", r"carefully.*instruction",
                     r"instruction.*carefully", r"read.*out loud"],
    },
    {
        "label": "Curiosity about adaptive difficulty",
        "patterns": [r"curiosit.*difficulty", r"how.*difficulty.*adjust",
                     r"adaptive.*changed", r"linked.*customiz"],
    },
    {
        "label": "Suggestion / unprompted feedback",
        "patterns": [r"suggest", r"highscore", r"trajectory color",
                     r"positioned.*low", r"positioned.*high"],
    },
]

UNCATEGORIZED_LABEL = "Other / uncategorized"


# ---------------------------------------------------------------------------
# 2.  HELPERS
# ---------------------------------------------------------------------------

def parse_observations(raw: str) -> list[str]:
    """
    Split a raw observation cell into individual numbered items.

    Observations are formatted as:
        "1. Text one\n2. Text two\n3. Text three"

    Returns a list of cleaned observation strings (without the leading number).
    """
    if not isinstance(raw, str) or not raw.strip():
        return []

    # Split on patterns like "1.", "2.", "3." at the start of a segment
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
# 3.  MAIN ANALYSIS
# ---------------------------------------------------------------------------

def analyse(csv_path: str, top_n: int | None = None) -> pd.DataFrame:
    """
    Load the CSV, analyse observations, and return a summary DataFrame.

    Parameters
    ----------
    csv_path : str
        Path to the CSV file.
    top_n : int or None
        If provided, return only the top N themes by participant count.

    Returns
    -------
    pd.DataFrame
        Summary table with columns:
        Theme | Participants | Percentage
    """
    # --- Load -----------------------------------------------------------
    # Row 0 is a duplicate header row; skip it and use row 1 as the real header
    df_raw = pd.read_csv(csv_path, header=1, encoding="utf-8-sig")
    df_raw.columns = df_raw.columns.str.strip()

    # Keep only rows that represent actual participants (non-empty Participant_ID)
    df = df_raw[df_raw["Participant_ID"].notna() &
                df_raw["Participant_ID"].str.strip().ne("")].copy()
    df.reset_index(drop=True, inplace=True)

    total_participants = len(df)
    print(f"\n{'='*60}")
    print(f"  Total participants found: {total_participants}")
    print(f"{'='*60}\n")

    # --- Parse & categorise --------------------------------------------
    # Map theme_label -> set of participant IDs that triggered it
    theme_to_users: dict[str, set] = defaultdict(set)

    for _, row in df.iterrows():
        participant = row["Participant_ID"].strip()
        observations = parse_observations(row.get("General_Observations", ""))

        for obs in observations:
            theme = match_theme(obs)
            theme_to_users[theme].add(participant)

    # --- Build summary DataFrame ----------------------------------------
    records = []
    for theme_label, users in theme_to_users.items():
        count = len(users)
        pct = round(count / total_participants * 100, 1)
        records.append({"Theme": theme_label, "Participants": count, "Percentage (%)": pct})

    summary = pd.DataFrame(records)
    summary.sort_values("Participants", ascending=False, inplace=True)
    summary.reset_index(drop=True, inplace=True)

    if top_n is not None:
        summary = summary.head(top_n)

    return summary, total_participants


def print_report(summary: pd.DataFrame, total: int, top_n: int | None) -> None:
    """Pretty-print the summary table to stdout."""
    title = "OBSERVATION THEME ANALYSIS"
    if top_n:
        title += f"  (Top {top_n})"
    print(f"\n{'='*60}")
    print(f"  {title}")
    print(f"  (out of {total} participants)")
    print(f"{'='*60}")
    print(f"{'#':<4} {'Theme':<52} {'n':>4}  {'%':>6}")
    print(f"{'-'*4} {'-'*52} {'-'*4}  {'-'*6}")

    for i, row in summary.iterrows():
        # Wrap long theme names
        theme = row["Theme"]
        parts = [theme[j:j+52] for j in range(0, len(theme), 52)]
        first = True
        for part in parts:
            if first:
                print(f"{i+1:<4} {part:<52} {row['Participants']:>4}  {row['Percentage (%)']:>5.1f}%")
                first = False
            else:
                print(f"{'':4} {part:<52}")

    print(f"{'='*60}\n")


# ---------------------------------------------------------------------------
# 4.  ENTRY POINT
# ---------------------------------------------------------------------------

def main():
    parser = argparse.ArgumentParser(
        description="Analyse General_Observations from a user-test CSV file."
    )
    parser.add_argument(
        "--csv",
        default=FILE_PATH,
        help=f"Path to the CSV file (default: FILE_PATH = '{FILE_PATH}')",
    )
    parser.add_argument(
        "--top",
        type=int,
        default=None,
        metavar="N",
        help="Show only the top N themes (default: all)",
    )
    parser.add_argument(
        "--export",
        default=None,
        metavar="OUTPUT.csv",
        help="Optional path to export the summary table as a CSV",
    )
    args = parser.parse_args()

    summary, total = analyse(args.csv, args.top)
    print_report(summary, total, args.top)

    if args.export:
        summary.to_csv(args.export, index=False)
        print(f"  Summary exported to: {args.export}\n")


if __name__ == "__main__":
    main()

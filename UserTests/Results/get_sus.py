"""
get_sus.py
----------
Calculates the System Usability Scale (SUS) score for each participant
from the Wave 2 user test results CSV, and generates both a bar chart
and a box plot of the scores.

Output:
    - SUS_Scores_Barchart.png  — per-participant bar chart
    - SUS_Boxplot.png          — distribution box plot

SUS scoring (Brooke, 1996):
  - Odd questions  (positive): contribution = response_value - 1
  - Even questions (negative): contribution = 5 - response_value
  - Final score = sum of contributions × 2.5  →  range 0–100

Grade interpretation (Bangor et al., 2009):
  ≥ 90.0  →  A+  Best Imaginable
  ≥ 85.0  →  A   Excellent
  ≥ 80.0  →  A-  Excellent
  ≥ 75.0  →  B+  Good
  ≥ 70.0  →  B   Good
  ≥ 65.0  →  C+  OK
  ≥ 57.0  →  C   OK
  ≥ 51.0  →  D   Poor
  < 51.0  →  F   Awful
"""

import csv
import os
import re
import matplotlib.pyplot as plt
from matplotlib.patches import Patch
from matplotlib.lines import Line2D

SUS_RESULTS_FILE = "UserTestResultsWave2.csv"
GRAPH_DIR = "Graphs"
SUS_GRAPH_FILE = "SUS_Scores_Barchart.png"
SUS_BOXPLOT_FILE = "SUS_Boxplot.png"

# Maps Likert response labels to their numeric value (1–5)
LIKERT_MAP = {
    "Strongly disagree": 1,
    "Disagree": 2,
    "Neutral": 3,
    "Agree": 4,
    "Strongly agree": 5,
}

# The 10 SUS question column headers, in order (odd = positive, even = negative)
SUS_QUESTIONS = [
    "I think that I would like to use this system frequently.",
    "I found the system unnecessarily complex.",
    "I thought the system was easy to use.",
    "I think that I would need the support of a technical person to be able to use .",
    "I found the various functions in the system were well integrated.",
    "I thought there was too much inconsistency in this system.",
    "I imagine that most people would learn to use this product very quickly.",
    "I found the product very awkward to use.",
    "I felt very confident using the product",
    "I needed to learn a lot of things before I could get going with this product.",
]

# Grade thresholds: (minimum_score, grade, adjective)
SUS_GRADES = [
    (90, "A+", "Best Imaginable"),
    (85, "A",  "Excellent"),
    (80, "A-", "Excellent"),
    (75, "B+", "Good"),
    (70, "B",  "Good"),
    (65, "C+", "OK"),
    (57, "C",  "OK"),
    (51, "D",  "Poor"),
    (0,  "F",  "Awful"),
]


def get_sus_grade(score):
    """Return the (grade, adjective) tuple for a given SUS score."""
    for threshold, grade, adjective in SUS_GRADES:
        if score >= threshold:
            return grade, adjective
        
    return "F", "Awful"


def calculate_sus_score(responses):
    """
    Compute the SUS score from a list of 10 Likert response strings.

    Returns the score (0–100, integer), or None if any response is unrecognised.
    """
    total = 0

    for i, response in enumerate(responses):
        value = LIKERT_MAP.get(response.strip())

        if value is None:
            return None
        
        if i % 2 == 0:  # odd questions (1,3,5,7,9) — positive items
            total += value - 1
        else:            # even questions (2,4,6,8,10) — negative items
            total += 5 - value

    return int(total * 2.5)


def format_participant_id(participant_id):
    """Return participant ID shifted by -1 when it contains a number."""
    text = str(participant_id).strip()
    numbers = re.findall(r"\d+", text)
    if not numbers:
        return text

    shifted = int(numbers[-1]) - 1
    return str(shifted)


def gen_sus_graph(results):
    """Generate a SUS score bar chart highlighting best/worst and average."""
    valid_results = [r for r in results if r["SUS"] is not None]

    if not valid_results:
        print("No valid SUS scores found. Graph was not generated.")
        return

    participant_ids = [format_participant_id(r["ID"]) for r in valid_results]
    scores = [r["SUS"] for r in valid_results]

    best_idx = scores.index(max(scores))
    worst_idx = scores.index(min(scores))

    colors = ["#5B9BD5"] * len(scores)
    colors[best_idx] = "#2ECC71"   # green for best score
    colors[worst_idx] = "#E74C3C"  # red for worst score

    avg = int(sum(scores) / len(scores))
    avg_grade, avg_adjective = get_sus_grade(avg)

    _, ax = plt.subplots(figsize=(10, 6))
    bars = ax.bar(participant_ids, scores, color=colors, edgecolor="white", linewidth=0.8)

    # Add score labels on top of bars.
    for i, (bar, score) in enumerate(zip(bars, scores)):
        weight = "bold" if i in (best_idx, worst_idx) else "normal"
        ax.text(
            bar.get_x() + bar.get_width() / 2,
            bar.get_height() + 1.2,
            f"{score}",
            ha="center",
            va="bottom",
            fontsize=10,
            fontweight=weight,
        )

    ax.axhline(avg, color="#F39C12", linestyle="--", linewidth=2)

    ax.set_title("System Usability Scale (SUS) Scores", fontsize=14, fontweight="bold", pad=15)
    ax.set_xlabel("Participant", fontsize=12)
    ax.set_ylabel("SUS Score", fontsize=12)
    ax.set_ylim(0, 105)
    ax.spines["top"].set_visible(False)
    ax.spines["right"].set_visible(False)

    legend_elements = [
        Patch(facecolor="#2ECC71", label=f"Best Score ({max(scores)})"),
        Patch(facecolor="#E74C3C", label=f"Worst Score ({min(scores)})"),
        Patch(facecolor="#5B9BD5", label="Other Scores"),
        Line2D(
            [0],
            [0],
            color="#F39C12",
            lw=2,
            linestyle="--",
            label=f"Average ({avg}, {avg_grade}, {avg_adjective})",
        ),
    ]
    ax.legend(
        handles=legend_elements,
        loc="upper center",
        bbox_to_anchor=(0.5, 1.02),
        ncol=2,
        fontsize=10,
    )

    script_dir = os.path.dirname(os.path.abspath(__file__))
    graph_dir = os.path.join(script_dir, GRAPH_DIR)
    os.makedirs(graph_dir, exist_ok=True)

    graph_path = os.path.join(graph_dir, SUS_GRAPH_FILE)
    plt.tight_layout()
    plt.savefig(graph_path, dpi=150)
    plt.close()

    print(f"Saved graph: {graph_path}")


def get_sus_results():
    """
    Read the CSV file and return a list of dicts with keys:
      'ID'        – participant identifier
      'SUS'       – numeric SUS score (int) or None if data is missing
      'Grade'     – letter grade string, or None
      'Adjective' – qualitative label, or None
    """
    script_dir = os.path.dirname(os.path.abspath(__file__))
    filepath = os.path.join(script_dir, SUS_RESULTS_FILE)

    results = []

    with open(filepath, newline="", encoding="utf-8") as f:
        reader = csv.DictReader(f)
        for row in reader:
            responses = [row[q] for q in SUS_QUESTIONS]
            score = calculate_sus_score(responses)

            if score is not None:
                grade, adjective = get_sus_grade(score)
            else:
                grade, adjective = None, None

            results.append({"ID": row["ID"], "SUS": score, "Grade": grade, "Adjective": adjective})

    return results


def gen_sus_boxplot(results):
    """Generate and save a SUS score box plot showing the score distribution.

    Parameters
    ----------
    results : list[dict]
        Output of :func:`get_sus_results`.  Only entries with a valid SUS
        score are included in the plot.
    """
    scores = [r["SUS"] for r in results if r["SUS"] is not None]

    if not scores:
        print("No valid SUS scores found. Box plot was not generated.")
        return

    avg = int(sum(scores) / len(scores))
    avg_grade, avg_adjective = get_sus_grade(avg)

    _, ax = plt.subplots(figsize=(6, 7))

    bp = ax.boxplot(
        scores,
        patch_artist=True,
        notch=False,
        widths=0.4,
        medianprops=dict(color="#F39C12", linewidth=2.5),
        whiskerprops=dict(linewidth=1.5),
        capprops=dict(linewidth=1.5),
        flierprops=dict(marker="o", markersize=6, linestyle="none",
                        markerfacecolor="#5B9BD5", markeredgecolor="#5B9BD5"),
    )

    bp["boxes"][0].set_facecolor("#5B9BD5")
    bp["boxes"][0].set_alpha(0.75)

    ax.axhline(avg, color="#F39C12", linestyle="--", linewidth=2)

    ax.set_xticks([1])
    ax.set_xticklabels(["SUS Score"], fontsize=12)
    ax.set_ylabel("SUS Score", fontsize=12)
    ax.set_ylim(0, 105)
    ax.set_title(
        "System Usability Scale (SUS) — Score Distribution",
        fontsize=14,
        fontweight="bold",
        pad=15,
    )
    ax.spines["top"].set_visible(False)
    ax.spines["right"].set_visible(False)

    legend_elements = [
        Patch(facecolor="#5B9BD5", alpha=0.75, label="SUS Scores"),
        Line2D([0], [0], color="#F39C12", lw=2.5, linestyle="--",
               label=f"Average ({avg}, {avg_grade}, {avg_adjective})"),
    ]
    ax.legend(
        handles=legend_elements,
        loc="upper center",
        bbox_to_anchor=(0.5, 1.02),
        ncol=2,
        fontsize=10,
    )

    script_dir = os.path.dirname(os.path.abspath(__file__))
    graph_dir = os.path.join(script_dir, GRAPH_DIR)
    os.makedirs(graph_dir, exist_ok=True)

    graph_path = os.path.join(graph_dir, SUS_BOXPLOT_FILE)
    plt.tight_layout()
    plt.savefig(graph_path, dpi=150)
    plt.close()
    print(f"Saved graph: {graph_path}")


def main():
    """Print SUS results table and generate a SUS score graph."""

    sus = get_sus_results()

    print(f"{'ID':<6} {'Score':>7}  {'Grade':<4}  Adjective")
    print("-" * 38)

    for r in sus:
        if r["SUS"] is not None:
            print(f"{format_participant_id(r['ID']):<6} {r['SUS']:>7}  {r['Grade']:<4}  {r['Adjective']}")
        else:
            print(f"{format_participant_id(r['ID']):<6} {'N/A':>7}")

    scores = [r["SUS"] for r in sus if r["SUS"] is not None]
    
    if scores:
        avg = int(sum(scores) / len(scores))
        avg_grade, avg_adjective = get_sus_grade(avg)

        print("-" * 38)
        print(f"{'Avg':<6} {avg:>7}  {avg_grade:<4}  {avg_adjective}")

    gen_sus_graph(sus)
    gen_sus_boxplot(sus)

if __name__ == "__main__":
    main()
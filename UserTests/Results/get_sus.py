"""
get_sus.py
----------
Calculates the System Usability Scale (SUS) score for each participant
from the Wave 2 user test results CSV.

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

SUS_RESULTS_FILE = "UserTestResultsWave2.csv"

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


def main():
    """Print a formatted SUS results table with scores, grades, and adjectives."""
    sus = get_sus_results()

    print(f"{'ID':<6} {'Score':>7}  {'Grade':<4}  Adjective")
    print("-" * 38)
    for r in sus:
        if r["SUS"] is not None:
            print(f"{r['ID']:<6} {r['SUS']:>7}  {r['Grade']:<4}  {r['Adjective']}")
        else:
            print(f"{r['ID']:<6} {'N/A':>7}")

    scores = [r["SUS"] for r in sus if r["SUS"] is not None]
    
    if scores:
        avg = int(sum(scores) / len(scores))
        avg_grade, avg_adjective = get_sus_grade(avg)
        print("-" * 38)
        print(f"{'Avg':<6} {avg:>7}  {avg_grade:<4}  {avg_adjective}")


if __name__ == "__main__":
    main()

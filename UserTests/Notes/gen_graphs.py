"""Generate bar chart visualizations for Country Fair VR user test results.

This script reads user test data from a CSV file, parses task completion
times in various formats (seconds, minutes, mixed notation), and produces
bar charts for each task. The best and worst times are highlighted in
green and red, respectively.

Usage:
    python gen_graphs.py

Output:
    PNG files saved to the ``Graphs/`` directory.
"""

import csv
import re
import os
import matplotlib.pyplot as plt
from matplotlib.patches import Patch

DIR_TO_SAVE_GRAPHS = "Graphs/"
USER_TEST_FILE = "UserTestWave2.csv"


def parse_time_to_seconds(time_str):
    """Convert a human-readable time string into total seconds.

    Supported formats:
        - ``"18 seconds"`` → 18.0
        - ``"3.13 minutes"`` → 193.0  (stopwatch notation: 3 min 13 s)
        - ``"1:45 minutes"`` → 105.0  (colon notation: 1 min 45 s)
        - ``"2. 18 minutes"`` → 138.0 (stray space tolerated)

    Parameters
    ----------
    time_str : str
        The raw time value as read from the CSV.

    Returns
    -------
    float
        Equivalent time in seconds.
    """
    time_str = time_str.strip()

    # Extract numeric part and unit
    if "second" in time_str.lower():
        num = re.findall(r"[\d.]+", time_str)
        return float(num[0])

    if "minute" in time_str.lower():
        # Remove "minutes" / "minute"
        cleaned = re.sub(r"\s*minutes?\s*", "", time_str).strip()

        # Handle "M:SS" format (e.g. "1:45")
        if ":" in cleaned:
            parts = cleaned.split(":")
            minutes = int(parts[0])
            seconds = int(parts[1])
            return minutes * 60 + seconds

        # Handle "M. SS" or "M.SS" format (e.g. "2. 18" or "3.13")
        # These are M minutes SS seconds (stopwatch notation)
        cleaned = cleaned.replace(" ", "")  # remove stray spaces like "2. 18" -> "2.18"
        if "." in cleaned:
            parts = cleaned.split(".")
            minutes = int(parts[0])
            seconds = int(parts[1])
            return minutes * 60 + seconds

        # Plain integer minutes
        return float(cleaned) * 60

    return 0.0


def read_data():
    """Read the user test CSV and extract per-task completion times.

    The CSV is expected to have two header rows followed by one row per
    participant.  Columns 1–3 contain the completion times for tasks 1–3.

    Returns
    -------
    tuple[list[str], list[float], list[float], list[float]]
        A tuple of ``(participants, task1_times, task2_times, task3_times)``
        where each time list contains values in seconds.
    """
    participants = []
    task1_times = []
    task2_times = []
    task3_times = []

    with open(USER_TEST_FILE, newline="", encoding="utf-8") as f:
        reader = csv.reader(f)
        header_row1 = next(reader)  # Column1, Column2, ...
        header_row2 = next(reader)  # Participant_ID, Task_1_Select_Mini_Game, ...

        for row in reader:
            if len(row) < 4:
                continue

            participant = row[0].strip()
            t1 = parse_time_to_seconds(row[1])
            t2 = parse_time_to_seconds(row[2])
            t3 = parse_time_to_seconds(row[3])
            participants.append(participant)
            task1_times.append(t1)
            task2_times.append(t2)
            task3_times.append(t3)

    return participants, task1_times, task2_times, task3_times


def format_seconds(seconds):
    """Format a duration as whole seconds.

    Parameters
    ----------
    seconds : float
        Duration in seconds.

    Returns
    -------
    str
        Formatted string in seconds, e.g. ``"45s"``.
    """
    return f"{seconds:.0f}s"


def gen_bar_chart(participants, times, title, ylabel, file_name, color_default="#5B9BD5"):
    """Generate and save a bar chart highlighting the best and worst times.

    The bar with the lowest time is coloured green and the bar with the
    highest time is coloured red.  All other bars use *color_default*.

    Parameters
    ----------
    participants : list[str]
        Labels for the x-axis (participant identifiers).
    times : list[float]
        Completion times in seconds (one per participant).
    title : str
        Chart title.
    ylabel : str
        Label for the y-axis.
    file_name : str
        Output file path for the saved PNG image.
    color_default : str, optional
        Hex colour used for bars that are neither best nor worst.
    """
    best_idx = times.index(min(times))
    worst_idx = times.index(max(times))
    avg_time = sum(times) / len(times)

    colors = [color_default] * len(times)
    colors[best_idx] = "#2ECC71"   # green for best time
    colors[worst_idx] = "#E74C3C"  # red for worst time

    _, ax = plt.subplots(figsize=(10, 6))
    bars = ax.bar(participants, times, color=colors, edgecolor="white", linewidth=0.8)

    # Add value labels on top of each bar
    for i, (bar, t) in enumerate(zip(bars, times)):
        label = format_seconds(t)
        weight = "bold" if i in (best_idx, worst_idx) else "normal"
        ax.text(bar.get_x() + bar.get_width() / 2, bar.get_height() + max(times) * 0.02,
                label, ha="center", va="bottom", fontsize=10, fontweight=weight)

    ax.axhline(avg_time, color="#F39C12", linestyle="--", linewidth=2)
 
    ax.set_title(title, fontsize=14, fontweight="bold", pad=15)
    ax.set_xlabel("Participant", fontsize=12)
    ax.set_ylabel(ylabel, fontsize=12)
    ax.set_ylim(0, max(times) * 1.18)
    ax.spines["top"].set_visible(False)
    ax.spines["right"].set_visible(False)

    # Legend for best and worst time
    legend_elements = [
        Patch(facecolor="#2ECC71", label=f"Best Time ({format_seconds(min(times))})"),
        Patch(facecolor="#E74C3C", label=f"Worst Time ({format_seconds(max(times))})"),
        Patch(facecolor="#F39C12", label=f"Average ({format_seconds(avg_time)})"),
    ]
    ax.legend(handles=legend_elements, loc="upper right", fontsize=10)

    plt.tight_layout()
    plt.savefig(file_name, dpi=150)
    plt.close()
    print(f"Saved: {file_name}")


def gen_task1_graph(participants, times):
    """Generate the bar chart for Task 1 (Select Mini-Game)."""
    file_name = DIR_TO_SAVE_GRAPHS + "Task1.png"
    
    gen_bar_chart(
        participants, times,
        title="Task 1 — Select Mini-Game (Time in Seconds)",
        ylabel="Time (seconds)",
        file_name=file_name,
        color_default="#5B9BD5",
    )


def gen_task2_graph(participants, times):
    """Generate the bar chart for Task 2 (Complete Frisbee)."""
    file_name = DIR_TO_SAVE_GRAPHS + "Task2.png"

    gen_bar_chart(
        participants, times,
        title="Task 2 — Complete Frisbee (Time in Seconds)",
        ylabel="Time (seconds)",
        file_name=file_name,
        color_default="#ED7D31",
    )


def gen_task3_graph(participants, times):
    """Generate the bar chart for Task 3 (Complete Archery)."""
    file_name = DIR_TO_SAVE_GRAPHS + "Task3.png"

    gen_bar_chart(
        participants, times,
        title="Task 3 — Complete Archery (Time in Seconds)",
        ylabel="Time (seconds)",
        file_name=file_name,
        color_default="#A855F7",
    )


def main():
    """Entry point: read data, generate all task charts, and print a summary."""
    os.makedirs(DIR_TO_SAVE_GRAPHS, exist_ok=True)
    participants, t1, t2, t3 = read_data()

    gen_task1_graph(participants, t1)
    gen_task2_graph(participants, t2)
    gen_task3_graph(participants, t3)

    print("\nAll graphs generated successfully!")

    for name, times in [("Task 1", t1), ("Task 2", t2), ("Task 3", t3)]:
        print(f"  {name}  —  Best: {format_seconds(min(times))}  |  Worst: {format_seconds(max(times))}")


if __name__ == "__main__":
    main()
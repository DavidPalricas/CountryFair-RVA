"""Generate bar chart and box plot visualizations for Country Fair VR user test results.

This script reads user test data from a CSV file, parses task completion
times in various formats (seconds, minutes, mixed notation), and produces
bar charts for each task and a combined box plot for all tasks.
The best and worst times are highlighted in green and red, respectively.

Usage:
    python gen_graphs.py

Output:
    PNG files saved to the ``Graphs/`` directory.
    - Task1.png, Task2.png, Task3.png  — individual bar charts
    - Task_Boxplot.png                 — combined box plot for all 3 tasks
"""

import csv
import re
import os
import matplotlib.pyplot as plt
from matplotlib.patches import Patch
from matplotlib.lines import Line2D

DIR_TO_SAVE_GRAPHS = "Graphs/"
USER_TEST_FILE = "UserTestWave2.csv"


def format_participant_label(participant):
    """Return participant label shifted by -1 when it contains a number."""
    numbers = re.findall(r"\d+", participant)
    if numbers:
        return str(int(numbers[-1]))
    
    return participant


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
        _ = next(reader)  # Column1, Column2, ...
        _= next(reader)  # Participant_ID, Task_1_Select_Mini_Game, ...

        for row in reader:
            if len(row) < 4:
                continue

            participant = format_participant_label(row[0].strip())
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
        Patch(facecolor="#5B9BD5", label="Others"),
        Line2D([0], [0], color="#F39C12", lw=2, linestyle="--", label=f"Average ({format_seconds(avg_time)})"),
    ]
    ax.legend(
        handles=legend_elements,
        loc="upper center",
        bbox_to_anchor=(0.5, 1.02),
        ncol=4,
        fontsize=10,
    )

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
    )


def gen_task2_graph(participants, times):
    """Generate the bar chart for Task 2 (Complete Frisbee)."""
    file_name = DIR_TO_SAVE_GRAPHS + "Task2.png"

    gen_bar_chart(
        participants, times,
        title="Task 2 — Complete Frisbee (Time in Seconds)",
        ylabel="Time (seconds)",
        file_name=file_name,
    )


def gen_task3_graph(participants, times):
    """Generate the bar chart for Task 3 (Complete Archery)."""
    file_name = DIR_TO_SAVE_GRAPHS + "Task3.png"

    gen_bar_chart(
        participants, times,
        title="Task 3 — Complete Archery (Time in Seconds)",
        ylabel="Time (seconds)",
        file_name=file_name,
    )


def gen_tasks_boxplot(task1_times, task2_times, task3_times):
    """Generate and save a combined box plot for all three task completion times.

    Each task is represented as a separate box in a single figure, making
    distribution comparison straightforward.  A logarithmic y-axis is used
    so that the compact Task 1 distribution is not crushed by the larger
    Task 2 values.  Both median and mean are shown with value annotations.

    Parameters
    ----------
    task1_times : list[float]
        Completion times in seconds for Task 1.
    task2_times : list[float]
        Completion times in seconds for Task 2.
    task3_times : list[float]
        Completion times in seconds for Task 3.
    """
    import statistics

    file_name = DIR_TO_SAVE_GRAPHS + "Task_Boxplot.png"

    data = [task1_times, task2_times, task3_times]
    labels = [
        "Task 1\nSelect Mini-Game",
        "Task 2\nComplete Frisbee\nMini-Game",
        "Task 3\nComplete Archery\nMini-Game",
    ]
    # Task 3 changed to green (#27AE60) for better differentiation
    colors = ["#5B9BD5", "#9B59B6", "#27AE60"]

    COLOR_MEDIAN = "#F39C12"   # orange
    COLOR_MEAN   = "#E74C3C"   # red

    _, ax = plt.subplots(figsize=(10, 6))

    bp = ax.boxplot(
        data,
        patch_artist=True,
        notch=False,
        widths=0.5,
        medianprops=dict(color=COLOR_MEDIAN, linewidth=2.5),
        whiskerprops=dict(linewidth=1.5),
        capprops=dict(linewidth=1.5),
        flierprops=dict(marker="o", markersize=6, linestyle="none"),
    )

    for patch, color in zip(bp["boxes"], colors):
        patch.set_facecolor(color)
        patch.set_alpha(0.75)

    for flier, color in zip(bp["fliers"], colors):
        flier.set_markerfacecolor(color)
        flier.set_markeredgecolor(color)

    # Logarithmic scale so Task 1 (very small values) is not squashed
    ax.set_yscale("log")

    # Draw mean lines
    box_width = 0.25  # half-width for the mean line segments
    medians = []
    means   = []
    for i, times in enumerate(data, start=1):
        med  = statistics.median(times)
        mean = sum(times) / len(times)
        medians.append(med)
        means.append(mean)

        # Mean horizontal line (dashed red)
        ax.plot([i - box_width, i + box_width], [mean, mean],
                color=COLOR_MEAN, linewidth=2.5, linestyle="--", zorder=5)

    ax.set_xticks([1, 2, 3])
    ax.set_xticklabels(labels, fontsize=11)

    # Add coloured "(median, mean)" annotation below each tick label.
    # We render it as three separate Text objects placed consecutively so each
    # segment can have its own colour.  matplotlib.text.Text.get_window_extent()
    # is used to chain them without overlap.
    fig = plt.gcf()
    fig.canvas.draw()          # needed so get_window_extent works
    renderer = fig.canvas.get_renderer()

    for i, (med, mean) in enumerate(zip(medians, means), start=1):
        parts = [
            (f"({med:.0f}s", "black", COLOR_MEDIAN),   # "(" black + value colored
            (", ", "black", "black"),
            (f"{mean:.0f}s)", "black", COLOR_MEAN),
        ]
        # We render using annotate with xycoords="data"/"axes fraction" combo.
        # Simpler: use fig.text in figure coordinates computed from ax transform.
        # Even simpler: render as a single ax.text per piece, x offset in points.

        # Determine x in data coords for this box
        # Compute the total label string to find its centre offset
        label_med  = f"{med:.0f}s"
        label_mean = f"{mean:.0f}s"

        char_w = 6.5   # approx pts per char at fontsize=9
        segments = [
            ("(", "black", False),
            (label_med, COLOR_MEDIAN, True),
            (", ", "black", False),
            (label_mean, COLOR_MEAN, True),
            (")", "black", False),
        ]
        total_pts = sum(len(s) * char_w for s, _, _ in segments)
        x_cursor  = -total_pts / 2   # start offset in points to centre the group

        for seg_text, seg_color, bold in segments:
            ax.annotate(
                seg_text,
                xy=(i, 0),
                xycoords=("data", "axes fraction"),
                xytext=(x_cursor, -42),
                textcoords="offset points",
                annotation_clip=False,
                fontsize=9,
                color=seg_color,
                fontweight="bold" if bold else "normal",
                va="top",
                ha="left",
            )
            x_cursor += len(seg_text) * char_w
    ax.set_ylabel("Time (seconds) — log scale", fontsize=12)
    ax.set_title(
        "Task Completion Times — Distribution by Task",
        fontsize=14,
        fontweight="bold",
        pad=15,
    )
    ax.spines["top"].set_visible(False)
    ax.spines["right"].set_visible(False)

    legend_elements = [
        Patch(facecolor=c, alpha=0.75, label=lbl.split("\n")[0] + " " + lbl.split("\n")[1])
        for c, lbl in zip(colors, labels)
    ] + [
        Line2D([0], [0], color=COLOR_MEDIAN, lw=2.5, label="Median"),
        Line2D([0], [0], color=COLOR_MEAN,   lw=2.5, linestyle="--", label="Mean"),
    ]
    ax.legend(
        handles=legend_elements,
        loc="upper center",
        bbox_to_anchor=(0.5, 1.02),
        ncol=5,
        fontsize=10,
    )

    plt.tight_layout()
    plt.subplots_adjust(bottom=0.22)   # extra room for the annotation row below x-ticks
    plt.savefig(file_name, dpi=150)
    plt.close()
    print(f"Saved: {file_name}")


def main():
    """Entry point: read data, generate all task charts, and print a summary."""
    os.makedirs(DIR_TO_SAVE_GRAPHS, exist_ok=True)
    participants, t1, t2, t3 = read_data()

    gen_task1_graph(participants, t1)
    gen_task2_graph(participants, t2)
    gen_task3_graph(participants, t3)
    gen_tasks_boxplot(t1, t2, t3)

    print("\nAll graphs generated successfully!")

    for name, times in [("Task 1", t1), ("Task 2", t2), ("Task 3", t3)]:
        print(f"  {name}  —  Best: {format_seconds(min(times))}  |  Worst: {format_seconds(max(times))}")


if __name__ == "__main__":
    main()
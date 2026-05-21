import sys
from datetime import datetime

log_file = r"C:\Users\User\Desktop\.NET\KitchenAidAI\.github\hooks\agentLog.txt"
output_file = r"C:\Users\User\Desktop\.NET\KitchenAidAI\lab4\agentLog4.txt"
start_date = "2025-05-17"

target_date = datetime.strptime(start_date, "%Y-%m-%d")

with open(log_file, "r", encoding="utf-8", errors="replace") as f:
    lines = f.readlines()

copying = False
result = []

for line in lines:
    try:
        date_str = line[14:24]
        line_date = datetime.strptime(date_str, "%Y-%m-%d")
        if line_date >= target_date:
            copying = True
    except ValueError:
        pass  

    if copying:
        result.append(line)

with open(output_file, "w", encoding="utf-8") as f:
    f.writelines(result)

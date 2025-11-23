from flask import Flask, request, jsonify
import pulp
import os

app = Flask(__name__)

@app.route('/health', methods=['GET'])
def health():
    return jsonify({"status": "healthy"}), 200

@app.route('/advisor/solve', methods=['POST'])
def solve():
    """
    Expects JSON payload:
    {
        "queries": [
            {"id": "q1", "costs": {"f1": 10, "f2": 20}, "memory": {"f1": 100, "f2": 200}, "weight": 1},
            ...
        ],
        "frameworks": ["f1", "f2"],
        "max_frameworks": 2,
        "max_memory": 1000000
    }
    """
    data = request.json
    queries = data.get('queries', [])
    frameworks = data.get('frameworks', [])
    max_frameworks = data.get('max_frameworks', 2)
    max_memory = data.get('max_memory', 1000000)

    # Decision variables
    # x[q][f] = 1 if query q is assigned to framework f
    x = pulp.LpVariable.dicts("x", ((q['id'], f) for q in queries for f in frameworks), cat='Binary')

    # y[f] = 1 if framework f is used
    y = pulp.LpVariable.dicts("y", frameworks, cat='Binary')

    prob = pulp.LpProblem("FrameworkSelection", pulp.LpMinimize)

    # Objective Function: Minimize total weighted cost
    prob += pulp.lpSum([
        q['weight'] * q['costs'].get(f, 999999) * x[(q['id'], f)]
        for q in queries for f in frameworks
    ])

    # Constraint 1: Each query must be assigned to exactly one framework
    for q in queries:
        prob += pulp.lpSum([x[(q['id'], f)] for f in frameworks]) == 1

    # Constraint 2: If a query is assigned to f, then f must be selected (x <= y)
    for q in queries:
        for f in frameworks:
            prob += x[(q['id'], f)] <= y[f]

    # Constraint 3: Limit total number of selected frameworks
    prob += pulp.lpSum([y[f] for f in frameworks]) <= max_frameworks

    # Constraint 4: Memory limit
    prob += pulp.lpSum([
        q['memory'].get(f, 0) * x[(q['id'], f)]
        for q in queries for f in frameworks
    ]) <= max_memory

    # Solve
    prob.solve()

    if pulp.LpStatus[prob.status] != 'Optimal':
        return jsonify({"status": "error", "message": "No optimal solution found"}), 400

    # Extract results
    assignment = {}
    for q in queries:
        for f in frameworks:
            if pulp.value(x[(q['id'], f)]) == 1:
                assignment[q['id']] = f
                break

    selected_frameworks = [f for f in frameworks if pulp.value(y[f]) == 1]
    total_cost = pulp.value(prob.objective)

    return jsonify({
        "status": "success",
        "objective_value": total_cost,
        "selected_frameworks": selected_frameworks,
        "assignments": assignment
    })

if __name__ == '__main__':
    app.run(host='0.0.0.0', port=5000)

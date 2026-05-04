my expriment idea

idea 1

- rock paper siccor like
- fire beats x, x beats circle, circle beats fire
- one block that beats the other blocks removes its entire row/column (above 4) or maybe its connections (above 5)

Here are some directions to add skill expression:

1. Require the attacker to form a group first (recommended starting point)

Instead of a single cell beating its neighbor, require that the attacking symbol forms a connected group of N (e.g., 3+) before it "activates" and clears adjacent beaten symbols. This is like Puyo Puyo's chain requirement but with the RPS twist on what gets cleared.

Forces the player to build up clusters intentionally before they trigger
Creates tension: do you build your group now or try to set up a chain?
Easy to prototype — just change ClearMatches to first find groups of 3+ same-type, then check if those groups are adjacent to their prey 2. Chain reactions as the scoring/skill mechanic

After a clear + gravity, re-check for new activations. Multi-step chains = exponentially more points. This rewards planning vertical setups so that clearing one layer exposes a new attacker group next to its prey below.

3. Restrict what clears — connected flood, not entire line

Right now you clear runs in a direction. Consider instead: the activated attacker clears all connected (flood-fill) cells of the beaten type that are adjacent to the attacking group. This makes the shape of what you build matter, not just linear alignment.

4. Smaller board, faster pressure

15x15 is very forgiving. A 6-wide, 12-tall board (classic Tetris/Puyo proportions) forces decisions faster and makes garbage/height matter.

5. Add a "neutral" or "wall" block

A 4th cell type that can't be beaten and can't attack. It just takes up space. Appears occasionally in pieces. Forces the player to manage dead weight and route around it.

6. Delayed activation / fuse mechanic

When an attacker group forms, it doesn't clear immediately — it "lights up" and clears after N more pieces drop. This gives the player time to extend the chain or the opponent time to disrupt (in multiplayer), and adds a planning horizon.

My recommendation for a first iteration: Combine ideas 1 + 2 + 4. Shrink the board to ~6x12, require a group of 3+ same-type to activate, and let gravity after clears trigger chain reactions. This alone transforms it from "place anything and stuff disappears" into "I need to build clusters of attackers next to clusters of prey, and set up cascades." The RPS element becomes the strategic layer on top — you're choosing which type to build up based on what's already on the board.

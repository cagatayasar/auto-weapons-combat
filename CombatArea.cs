using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class CombatArea
{
    //------------------------------------------------------------------------
    public List<List<CombatWeapon>> playerRowsList = new List<List<CombatWeapon>>();
    public List<CombatWeapon> playerCombatWeapons = new List<CombatWeapon>();
    public List<List<CombatWeapon>> enemyRowsList = new List<List<CombatWeapon>>();
    public List<CombatWeapon> enemyCombatWeapons = new List<CombatWeapon>();
    public int rowCapacity = 3;

    //------------------------------------------------------------------------
    public static Vec2[][] combatAreaPositions;
    public static float combatAreaWidth;
    public static float combatAreaHeight;

    public static int[][] rowCoordsX = new int[][]
    {
        new int[] {},
        new int[] { 0 },
        new int[] { 1, -1 },
        new int[] { 2, 0, -2 },
    };

    public static int[][] colCoordsY = new int[][]
    {
        new int[] {},
        new int[] { 0 },
        new int[] { -1, 1 },
        new int[] { -2, 0, 2 },
    };

    //------------------------------------------------------------------------
    public event Action<bool> onCoordsUpdated;

    //------------------------------------------------------------------------
    public static void SetCombatAreaPositions(float width, float height, float[] rowScales, float[] colScales)
    {
        combatAreaWidth = width;
        combatAreaHeight = height;
        combatAreaPositions = new Vec2[5][];
        for (int i = 0; i < 5; i++)
            combatAreaPositions[i] = new Vec2[5];

        var halfWidth = combatAreaWidth / 2f;
        for (int i = 0; i < 5; i++) {
            for (int j = 0; j < 5; j++) {
                combatAreaPositions[i][j] = new Vec2(
                    halfWidth * rowScales[i],
                    height * colScales[j]
                );
            }
        }
    }

    //------------------------------------------------------------------------
    public void UpdateCoords(bool isPreparationPhase, bool isPlayer, List<List<CombatWeapon>> rowsList = null)
    {
        rowsList ??= isPlayer ? playerRowsList : enemyRowsList;

        // DELETE EMPTY ROWS
        var listsToRemove = new List<List<CombatWeapon>>();
        for (int i = rowsList.Count - 1; i >= 0; i--) {
            if (rowsList[i].Count == 0) {
                rowsList.RemoveAt(i);
            }
        }

        foreach (List<CombatWeapon> row in rowsList) {
            row.Sort((x, y) => x.coordY - y.coordY);
        }
        rowsList.Sort((x, y) => (y[0].coordX - x[0].coordX) * isPlayer.ToMultiplier());

        var coordsX = isPreparationPhase ? rowCoordsX[rowsList.Count] : rowCoordsX[3];
        for (int i = 0; i < rowsList.Count; i++)
        {
            var row = rowsList[i];
            var coordsY = colCoordsY[row.Count];
            for (int j = 0; j < row.Count; j++)
            {
                var cw = row[j];
                cw.coordX = coordsX[i] * isPlayer.ToMultiplier();
                cw.coordY = coordsY[j];
                cw.rowNumber = i + 1;
                cw.positionFromBottom = row[j].coordY + 3;
            }
        }

        onCoordsUpdated?.Invoke(isPlayer);
    }

    //------------------------------------------------------------------------
    public void PullCombatWeapon(bool isPreparationPhase, List<List<CombatWeapon>> rowsList, CombatWeapon combatWeapon)
    {
        if (rowsList[combatWeapon.rowNumber - 1].Count == 1 || rowsList.Count != 3) {
            combatWeapon.coordX = rowsList[0][0].coordX + combatWeapon.isPlayer.ToMultiplier();
            rowsList.Add(new List<CombatWeapon> { combatWeapon });
            rowsList[combatWeapon.rowNumber - 1].Remove(combatWeapon);
        } else {
            if (combatWeapon.rowNumber - 1 == 0) return;
            if (rowsList[0].Count == rowCapacity) {
                if (rowsList[1].Count != rowCapacity) {
                    combatWeapon.coordX = rowsList[1][0].coordX;
                    rowsList[1].Add(combatWeapon);
                    rowsList[combatWeapon.rowNumber - 1].Remove(combatWeapon);
                }
            } else {
                combatWeapon.coordX = rowsList[0][0].coordX;
                rowsList[0].Add(combatWeapon);
                rowsList[combatWeapon.rowNumber - 1].Remove(combatWeapon);
            }
        }
        UpdateCoords(isPreparationPhase, combatWeapon.isPlayer);
    }

    //------------------------------------------------------------------------
    public void PushCombatWeapon(bool isPreparationPhase, List<List<CombatWeapon>> rowsList, CombatWeapon combatWeapon)
    {
        if (rowsList[combatWeapon.rowNumber - 1].Count == 1 || rowsList.Count != 3) {
            combatWeapon.coordX = rowsList[rowsList.Count-1][0].coordX - combatWeapon.isPlayer.ToMultiplier();
            rowsList.Add(new List<CombatWeapon> { combatWeapon });
            rowsList[combatWeapon.rowNumber - 1].Remove(combatWeapon);
        } else {
            if (combatWeapon.rowNumber == 3) return;
            if (rowsList[2].Count == rowCapacity) {
                if (rowsList[1].Count != rowCapacity) {
                    combatWeapon.coordX = rowsList[1][0].coordX;
                    rowsList[1].Add(combatWeapon);
                    rowsList[combatWeapon.rowNumber - 1].Remove(combatWeapon);
                }
            } else {
                combatWeapon.coordX = rowsList[2][0].coordX;
                rowsList[2].Add(combatWeapon);
                rowsList[combatWeapon.rowNumber - 1].Remove(combatWeapon);
            }
        }
        UpdateCoords(isPreparationPhase, combatWeapon.isPlayer);
    }

    //------------------------------------------------------------------------
    public void SetKnockbackPosition(bool isPreparationPhase, CombatWeapon cw)
    {
        var rowsList = cw.isPlayer ? playerRowsList : enemyRowsList;
        if (rowsList.Count >= cw.rowNumber + 1) {
            if (rowsList[cw.rowNumber].Count == rowCapacity) return;
            cw.coordX = rowsList[cw.rowNumber][0].coordX;
            rowsList[cw.rowNumber].Add(cw);
            rowsList[cw.rowNumber - 1].Remove(cw);
        } else {
            cw.coordX = cw.coordX - cw.isPlayer.ToMultiplier();
            rowsList.Add(new List<CombatWeapon> { cw });
            rowsList[cw.rowNumber - 1].Remove(cw);
        }
        UpdateCoords(isPreparationPhase, cw.isPlayer);
    }
}

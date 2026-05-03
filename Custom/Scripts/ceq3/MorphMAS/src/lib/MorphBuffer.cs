using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Utils.DictUtils;
using System;

namespace MorphMAS
{
    public class MorphBuffer
    {
        public Dictionary<int, Vector3> deltas;
        public Dictionary<string, Dictionary<DAZMorphFormulaTargetType, float>> formulas;

        public MorphBuffer(
            Dictionary<int, Vector3> deltas = null,
            Dictionary<string, Dictionary<DAZMorphFormulaTargetType, float>> formulas = null
        )
        {
            if (deltas == null) deltas = new Dictionary<int, Vector3>();
            this.deltas = deltas;
            if (formulas == null) formulas = new Dictionary<string, Dictionary<DAZMorphFormulaTargetType, float>>();
            this.formulas = formulas;
        }

        public void AddDelta(int vertex, Vector3 delta, float morphValue = 1)
        {
            if (!deltas.ContainsKey(vertex))
                deltas[vertex] = delta * morphValue;
            else
                deltas[vertex] += delta * morphValue;
        }

        public DAZMorphVertex[] ExportDeltas()
        {
            List<DAZMorphVertex> deltasList = new List<DAZMorphVertex>();
            foreach (KeyValuePair<int, Vector3> vertexDeltaPair in deltas)
            {
                deltasList.Add(new DAZMorphVertex
                {
                    vertex = vertexDeltaPair.Key,
                    delta = vertexDeltaPair.Value,
                });
            }
            return deltasList.ToArray();
        }

        public void AddFormula(string target, DAZMorphFormulaTargetType targetType, float multiplier, float morphValue = 1)
        {
            if (!formulas.ContainsKey(target))
                formulas[target] = new Dictionary<DAZMorphFormulaTargetType, float>();
            if (!formulas[target].ContainsKey(targetType))
                formulas[target][targetType] = multiplier * morphValue;
            else
                formulas[target][targetType] += multiplier * morphValue;
        }

        public void IterFormulas(Action<string, DAZMorphFormulaTargetType, float> callback)
        {
            foreach (KeyValuePair<string, Dictionary<DAZMorphFormulaTargetType, float>> targetFormulasPair in formulas)
            {
                foreach (KeyValuePair<DAZMorphFormulaTargetType, float> targetTypeMultiplierPair in targetFormulasPair.Value)
                {
                    callback(targetFormulasPair.Key, targetTypeMultiplierPair.Key, targetTypeMultiplierPair.Value);
                }
            }
        }

        public DAZMorphFormula[] ExportFormulas()
        {
            List<DAZMorphFormula> formulasList = new List<DAZMorphFormula>();
            IterFormulas((target, targetType, multiplier) =>
                formulasList.Add(new DAZMorphFormula
                {
                    target = target,
                    targetType = targetType,
                    multiplier = multiplier,
                })
            );
            return formulasList.ToArray();
        }

        public void ApplyFilter(MorphFilter filter, out MorphBuffer includedMorph, out MorphBuffer excludedMorph)
        {

            Dictionary<int, Vector3> includedMorphDeltas = new Dictionary<int, Vector3>();
            Dictionary<int, Vector3> excludedMorphDeltas = DictUtils.CloneDict(deltas);

            foreach (int vertex in filter.vertices)
            {
                if (excludedMorphDeltas.ContainsKey(vertex))
                {
                    includedMorphDeltas[vertex] = excludedMorphDeltas[vertex];
                    excludedMorphDeltas.Remove(vertex);
                }
            }

            Dictionary<string, Dictionary<DAZMorphFormulaTargetType, float>> includedMorphFormulas = (
                new Dictionary<string, Dictionary<DAZMorphFormulaTargetType, float>>()
            );
            Dictionary<string, Dictionary<DAZMorphFormulaTargetType, float>> excludedMorphFormulas = (
                DictUtils.CloneDictDeep(formulas)
            );

            foreach (string bone in filter.bones)
            {
                if (excludedMorphFormulas.ContainsKey(bone))
                {
                    includedMorphFormulas[bone] = excludedMorphFormulas[bone];
                    excludedMorphFormulas.Remove(bone);
                }
            }

            includedMorph = new MorphBuffer(includedMorphDeltas, includedMorphFormulas);
            excludedMorph = new MorphBuffer(excludedMorphDeltas, excludedMorphFormulas);
        }

        public MorphBuffer Clone()
        {
            return new MorphBuffer(DictUtils.CloneDict(deltas), DictUtils.CloneDictDeep(formulas));
        }

        public MorphBuffer ApplyOffsetOn(Vector3 offset, MorphFilter filter)
        {
            MorphBuffer offsettedBuffer = Clone();

            foreach (int vertex in filter.vertices)
            {
                offsettedBuffer.AddDelta(vertex, offset);
            }

            foreach (string bone in filter.bones)
            {
                if (offset.x != 0) offsettedBuffer.AddFormula(bone, DAZMorphFormulaTargetType.BoneCenterX, offset.x);
                if (offset.y != 0) offsettedBuffer.AddFormula(bone, DAZMorphFormulaTargetType.BoneCenterY, offset.y);
                if (offset.z != 0) offsettedBuffer.AddFormula(bone, DAZMorphFormulaTargetType.BoneCenterZ, offset.z);
            }

            return offsettedBuffer;
        }

        public MorphBuffer CombineWith(MorphBuffer otherBuffer)
        {
            MorphBuffer combinedBuffer = Clone();

            foreach (KeyValuePair<int, Vector3> vertexDeltaPair in otherBuffer.deltas)
            {
                combinedBuffer.AddDelta(vertexDeltaPair.Key, vertexDeltaPair.Value);
            }
            otherBuffer.IterFormulas((target, targetType, multiplier) =>
            {
                combinedBuffer.AddFormula(target, targetType, multiplier);
            });

            return combinedBuffer;
        }

        public DAZMorph ExportMorph(string morphName, string morphCategory, DAZMorphBank morphBank = null, bool isPoseControl = false)
        {
            DAZMorphVertex[] deltas = ExportDeltas();
            DAZMorph exportedMorph = new DAZMorph
            {
                morphName = morphName,
                displayName = morphName,
                group = morphCategory,
                region = morphCategory,
                min = 0,
                max = 1,
                visible = true,
                disable = false,
                isPoseControl = isPoseControl,
                deltas = deltas,
                numDeltas = deltas.Length,
                formulas = ExportFormulas(),
            };
            if (morphBank != null) exportedMorph.morphBank = morphBank;
            return exportedMorph;
        }
    }
}

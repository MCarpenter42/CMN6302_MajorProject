namespace NeoCambion
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    namespace Maths
    {
        namespace Matrices
        {
            public static class SquareMatrixUtility
            {
                public static float Determinant(float[,] matData)
                {
                    if (matData.GetLength(0) == matData.GetLength(1))
                    {
                        int size = matData.GetLength(0);
                        if (size == 2)
                        {
                            return matData[0,0] * matData[1, 1] - matData[1, 0] * matData[0, 1];
                        }
                        else if (size > 2)
                        {
                            float result = 0.0f;

                            int subSize = size - 1;
                            List<float[,]> subMatrices = new List<float[,]>();
                            for (int i = 0; i < size; i++)
                            {
                                float[,] subMatData = new float[subSize,subSize];
                                for (int j = 0; j < subSize; j++)
                                {
                                    int jAdj = j >= i ? j + 1 : j;
                                    for (int k = 0; k < subSize; k++)
                                    {
                                        subMatData[j,k] = matData[jAdj,k+1];
                                    }
                                }
                                subMatrices.Add(subMatData);
                            }

                            for (int i = 0; i < size; i++)
                            {
                                float sign = (i % 2) == 0 ? 1 : -1;
                                result += sign * matData[i,0] * Determinant(subMatrices[i]);
                            }

                            return result;
                        }
                        else if (size == 1)
                        {
                            return matData[0, 0];
                        }
                        else
                        {
                            return 0.0f;
                        }
                    }
                    else
                    {
                        return float.MinValue;
                    }
                }

                public static float Determinant(SquareMatrix matrix)
                {
                    return Determinant(matrix.matrix);
                }

                public static float[,] Inverse(float[,] matData)
                {
                    if (matData.GetLength(0) == matData.GetLength(1))
                    {
                        int size = matData.GetLength(0);
                        float det = Determinant(matData);

                        float[,] preAdjugate = new float[size, size];
                        float[,] inverse = new float[matData.GetLength(0), matData.GetLength(1)];

                        int subSize = size - 1;
                        float[,] subMatData = new float[subSize, subSize];
                        for (int i = 0; i < size; i++)
                        {
                            for (int j = 0; j < size; j++)
                            {
                                for (int x = 0; x < subSize; x++)
                                {
                                    int x2 = (x >= i) ? x + 1 : x;
                                    for (int y = 0; y < subSize; y++)
                                    {
                                        int y2 = (y >= j) ? y + 1 : y;
                                        subMatData[x,y] = matData[x2,y2];
                                    }
                                }
                                preAdjugate[i, j] = Determinant(subMatData) * ((i + j) % 2 == 0 ? 1.0f : -1.0f);
                            }
                        }

                        for (int i = 0; i < size; i++)
                        {
                            for (int j = 0; j < size; j++)
                            {
                                inverse[i,j] = preAdjugate[j,i] / det;
                            }
                        }

                        return inverse;
                    }
                    else
                    {
                        return null;
                    }
                }

                public static float[,] Inverse(SquareMatrix matrix)
                {
                    return Inverse(matrix.matrix);
                }
            }

            public struct SquareMatrix
            {
                private int _size;
                public int size { get { return _size; } }
                private float[,] _matrix;
                public float[,] matrix { get { return _matrix; } }
                public float determinant { get { return SquareMatrixUtility.Determinant(matrix); } }
                public float[,] inverse { get { return SquareMatrixUtility.Inverse(matrix); } }

                public SquareMatrix(int size)
                {
                    _size = size >= 0 ? size : -size;
                    if (_size < 2)
                        _size = 2;
                    _matrix = new float[size,size];
                }
                
                public SquareMatrix(int size, float[] values)
                {
                    _size = size >= 0 ? size : -size;
                    if (_size < 2)
                        _size = 2;
                    _matrix = new float[size,size];
                    SetValues(values);
                }
                
                public SquareMatrix(int size, List<float> values)
                {
                    _size = size >= 0 ? size : -size;
                    if (_size < 2)
                        _size = 2;
                    _matrix = new float[size,size];
                    SetValues(values);
                }
                
                public SquareMatrix(int size, float[,] inputMatrix)
                {
                    _size = size >= 0 ? size : -size;
                    if (_size < 2)
                        _size = 2;
                    _matrix = new float[size,size];
                    SetValues(inputMatrix);
                }

                public void SetValues(float[] input)
                {
                    int n = 0;
                    for (int i = 0; i < matrix.GetLength(0); i++)
                    {
                        for (int j = 0; j < matrix.GetLength(1); j++)
                        {
                            if (n < input.Length)
                            {
                                matrix[j, i] = input[n];
                            }
                            else
                            {
                                return;
                            }
                            n++;
                        }
                    }
                }

                public void SetValues(List<float> input)
                {
                    int n = 0;
                    for (int i = 0; i < matrix.GetLength(0); i++)
                    {
                        for (int j = 0; j < matrix.GetLength(1); j++)
                        {
                            if (n < input.Count)
                            {
                                matrix[j, i] = input[n];
                            }
                            else
                            {
                                return;
                            }
                            n++;
                        }
                    }
                }

                public void SetValues(float[,] inputMatrix)
                {
                    for (int i = 0; i < size; i++)
                    {
                        for (int j = 0; j < size; j++)
                        {
                            if (i < inputMatrix.GetLength(1) && i < inputMatrix.GetLength(0))
                            {
                                _matrix[i,j] = inputMatrix[i,j];
                            }
                            else
                            {
                                _matrix[i, j] = 0.0f;
                            }
                        }
                    }
                }
            }
        }
    }
}
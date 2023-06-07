namespace NeoCambion.Unity.Editor
{
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEditor;

    using NeoCambion.Collections;
    using NeoCambion.Collections.Unity;

    public delegate void MenuReturn<T>(T returnVal);

    public struct DropdownMenuItem
    {
        public GUIContent content;
        public DropdownMenuItem[] submenu;
        public int index;
        public bool enabled;

        public DropdownMenuItem(GUIContent content = null, int index = -1, bool enabled = true) : this()
        {
            this.index = content == null ? -1 : (enabled ? index : -1);
            this.content = content;
            this.enabled = enabled;
            submenu = null;
        }

        public DropdownMenuItem(GUIContent content, DropdownMenuItem[] submenu, int index = -1) : this()
        {
            if (submenu == null || submenu.Length < 1)
                this = new DropdownMenuItem(content, index);
            else
            {
                this.content = content;
                this.submenu = submenu;
                this.index = index;
                enabled = true;
            }
        }

        public DropdownMenuItem(string label = null, int index = -1, bool enabled = true) : this()
        {
            this.index = label == null ? -1 : (enabled ? index : -1);
            content = new GUIContent(label);
            this.enabled = enabled;
            submenu = null;
        }

        public DropdownMenuItem(string label, string[] submenu, int index = -1) : this()
        {
            if (submenu == null || submenu.Length < 1)
                this = new DropdownMenuItem(content, index);
            else
            {
                content = new GUIContent(label);
                this.submenu = new DropdownMenuItem[submenu.Length];
                for (int i = 0; i < submenu.Length; i++)
                    this.submenu[i] = new DropdownMenuItem(submenu[i], i);
                this.index = index;
                enabled = true;
            }
        }
    }

    public class EditorStylesExtras
    {
        private static bool darkTheme { get { return GUI.skin.label.normal.textColor.ApproximatelyEquals(new Color(0.824f, 0.824f, 0.824f, 1.000f), 0.005f); } }

        private static GUIStyle _noMarginsNoPadding = null;
        public static GUIStyle noMarginsNoPadding
        {
            get
            {
                if (_noMarginsNoPadding == null)
                {
                    _noMarginsNoPadding = new GUIStyle(EditorStyles.inspectorFullWidthMargins)
                    {
                        margin = new RectOffset(0, 0, 0, 0),
                        padding = new RectOffset(0, 0, 0, 0)
                    };
                }
                return _noMarginsNoPadding;
            }
        }

        private static GUIStyle _foldoutLabel = null;
        public static GUIStyle foldoutLabel
        {
            get
            {
                if (_foldoutLabel == null)
                {
                    _foldoutLabel = new GUIStyle(EditorStyles.foldoutHeader)
                    {
                        margin = new RectOffset(0, 0, 0, 0)
                    };
                }
                return _foldoutLabel;
            }
        }

        private static GUIStyle _foldoutInternal = null;
        public static GUIStyle foldoutInternal
        {
            get
            {
                if (_foldoutInternal == null)
                {
                    _foldoutInternal = new GUIStyle()
                    {
                        margin = new RectOffset(0, 0, 0, 0),
                        padding = new RectOffset(5, 0, 0, 0),
                        stretchHeight = true
                    };
                }
                return _foldoutInternal;
            }
        }

        private static GUIStyle _labelCentred = null;
        public static GUIStyle labelCentred
        {
            get
            {
                if (_labelCentred == null)
                {
                    _labelCentred = new GUIStyle(GUI.skin.label)
                    {
                        alignment = TextAnchor.MiddleCenter
                    };
                }
                return _labelCentred;
            }
        }

        private static GUIStyle _labelCentredLeft = null;
        public static GUIStyle labelCentredLeft
        {
            get
            {
                if (_labelCentredLeft == null)
                {
                    _labelCentredLeft = new GUIStyle(GUI.skin.label)
                    {
                        alignment = TextAnchor.MiddleLeft
                    };
                }
                return _labelCentredLeft;
            }
        }

        private static GUIStyle _labelCentredRight = null;
        public static GUIStyle labelCentredRight
        {
            get
            {
                if (_labelCentredRight == null)
                {
                    _labelCentredRight = new GUIStyle(GUI.skin.label)
                    {
                        alignment = TextAnchor.MiddleRight
                    };
                }
                return _labelCentredRight;
            }
        }

        public static GUIStyle textButtonRed
        {
            get
            {
                return ColouredTextButton(
                    new Color(1.000f, 0.700f, 0.700f),
                    new Color(1.000f, 0.200f, 0.200f),
                    new Color(0.560f, 0.120f, 0.120f),
                    new Color(0.820f, 0.120f, 0.120f)
                    );
            }
        }

        public static GUIStyle textButtonGreen
        {
            get
            {
                return ColouredTextButton(
                    new Color(0.700f, 1.000f, 0.700f),
                    new Color(0.200f, 1.000f, 0.200f),
                    new Color(0.120f, 0.560f, 0.120f),
                    new Color(0.120f, 0.820f, 0.120f)
                );
            }
        }

        public static GUIStyle textButtonBlue
        {
            get
            {
                return ColouredTextButton(
                    new Color(0.700f, 0.700f, 1.000f),
                    new Color(0.200f, 0.200f, 1.000f),
                    new Color(0.120f, 0.120f, 0.560f),
                    new Color(0.120f, 0.120f, 0.820f)
                );
            }
        }

        public static GUIStyle textButtonLightBlue
        {
            get
            {
                return ColouredTextButton(
                    new Color(0.700f, 1.000f, 1.000f),
                    new Color(0.200f, 1.000f, 1.000f),
                    new Color(0.120f, 0.560f, 0.560f),
                    new Color(0.120f, 0.820f, 0.820f)
                );
            }
        }

        public static GUIStyle textButtonOrange
        {
            get
            {
                return ColouredTextButton(
                    new Color(1.000f, 0.850f, 0.700f),
                    new Color(1.000f, 0.600f, 0.200f),
                    new Color(0.560f, 0.340f, 0.120f),
                    new Color(0.820f, 0.470f, 0.120f)
                );
            }
        }

        public static GUIStyle textButtonPurple
        {
            get
            {
                return ColouredTextButton(
                    new Color(0.850f, 0.700f, 1.000f),
                    new Color(0.700f, 0.400f, 1.000f),
                    new Color(0.340f, 0.120f, 0.560f),
                    new Color(0.470f, 0.120f, 0.820f)
                );
            }
        }

        public static GUIStyle ColouredTextButton(Color clrNormalDark, Color clrHoverDark, Color clrNormalLight, Color clrHoverLight, FontStyle fontStyle = FontStyle.Bold)
        {
            return new GUIStyle(GUI.skin.button)
            {
                fontStyle = fontStyle,
                normal = new GUIStyleState()
                {
                    background = GUI.skin.button.normal.background,
                    scaledBackgrounds = GUI.skin.button.normal.scaledBackgrounds,
                    textColor = darkTheme ? clrNormalDark : clrNormalLight
                },
                hover = new GUIStyleState()
                {
                    background = GUI.skin.button.hover.background,
                    scaledBackgrounds = GUI.skin.button.hover.scaledBackgrounds,
                    textColor = darkTheme ? clrHoverDark : clrHoverLight
                }
            };
        }
    }

    public static class EditorElements
    {
        private static GUIStyle _paragraphStyle;
        public static GUIStyle paragraphStyle
        {
            get
            {
                if (_paragraphStyle == null)
                {
                    _paragraphStyle = new GUIStyle(EditorStylesExtras.noMarginsNoPadding)
                    {
                        alignment = TextAnchor.MiddleLeft,
                        fixedHeight = 0,
                        fixedWidth = 0,
                        fontStyle = FontStyle.Normal,
                        normal = new GUIStyleState()
                        {
                            textColor = GUI.skin.label.normal.textColor
                        },
                        richText = true,
                        wordWrap = true
                    };
                }
                return _paragraphStyle;
            }
        }

        public struct ParagraphRect
        {
            public Vector2 position;
            public Vector2 size;
            public Vector2 center { get { return position + size / 2.0f; } set { this = SetCenter(value); } }
            public float x { get { return position.x; } set { this = SetX(value); } }
            public float y { get { return position.y; } set { this = SetY(value); } }
            public float width { get { return size.x; } set { this = SetWidth(value); } }
            public float height { get { return size.y; } set { this = SetHeight(value); } }

            public int padLeft;
            public int padRight;
            public int[] padding { get { return new int[] { padLeft, padRight }; } }
            public Vector2 positionPadded { get { return new Vector2(position.x + padLeft, position.y); } }
            public Vector2 sizePadded { get { return new Vector2(size.x - padLeft - padRight, size.y); } }
            public float xPadded { get { return positionPadded.x; } }
            public float yPadded { get { return positionPadded.y; } }
            public float widthPadded { get { return sizePadded.x; } }
            public float heightPadded { get { return sizePadded.y; } }

            public Rect rectBase { get { return new Rect(position, size); } }
            public Rect rectPadded { get { return new Rect(positionPadded, sizePadded); } }

            public ParagraphRect(float x, float y, float width, float height, int padLeft, int padRight)
            {
                this.position = new Vector2(x, y);
                this.size = new Vector2(width, height);
                this.padLeft = padLeft;
                this.padRight = padRight;
            }
            
            public ParagraphRect(Vector2 position, Vector2 size, int padLeft, int padRight)
            {
                this.position = position;
                this.size = size;
                this.padLeft = padLeft;
                this.padRight = padRight;
            }

            private ParagraphRect SetCenter(Vector2 center)
            {
                Vector2 diff = center - this.center;
                return new ParagraphRect(position + diff, size, padLeft, padRight);
            }

            private ParagraphRect SetX(float x)
            {
                return new ParagraphRect(new Vector2(position.x + x, position.y), size, padLeft, padRight);
            }

            private ParagraphRect SetY(float y)
            {
                return new ParagraphRect(new Vector2(position.x, position.y + y), size, padLeft, padRight);
            }

            private ParagraphRect SetWidth(float w)
            {
                return new ParagraphRect(position, new Vector2(size.x + w, size.y), padLeft, padRight);
            }

            private ParagraphRect SetHeight(float h)
            {
                return new ParagraphRect(position, new Vector2(size.x, size.y + h), padLeft, padRight);
            }
        }

        public class DropdownMenu
        {
            public DropdownMenuItem[] items;

            public void SetItems(MenuReturn<int[]> returnFunc, DropdownMenuItem[] items, int[] activeItem = null)
            {
                
            }

            public void Draw(Rect position, MenuReturn<int> returnFunc, DropdownMenuItem[] items, int[] activeItem = null)
            {

            }
        }

        /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

        public static void BeginSubSection(float minHeight, float maxHeight, int barWidth = 3)
        {
            if (barWidth < 1)
                barWidth = 1;
            int lPad = barWidth >= 12 ? 0 : 6 - (barWidth - (barWidth % 2)) / 2;
            GUIStyle tempStyle = new GUIStyle(EditorStylesExtras.foldoutInternal)
            {
                padding = new RectOffset(lPad, 0, 0, 0),
                stretchHeight = maxHeight <= 0
            };

            if (maxHeight <= 0)
                EditorGUILayout.BeginHorizontal(tempStyle, GUILayout.MinHeight(minHeight));
            else
                EditorGUILayout.BeginHorizontal(tempStyle, GUILayout.MinHeight(minHeight), GUILayout.MaxHeight(maxHeight));
            GreyRect(barWidth, 0, false, true);
            EditorGUILayout.BeginVertical(tempStyle);
            EditorGUILayout.Space(4.0f);
        }

        public static void EndSubSection()
        {
            EditorGUILayout.Space(4.0f);
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
        }

        /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

        public static void Header(Rect position, string text, int fontSize = 15, TextAnchor alignment = TextAnchor.MiddleCenter, bool bold = true)
        {
            GUIStyle headerStyle = new GUIStyle()
            {
                alignment = alignment,
                normal = new GUIStyleState()
                {
                    textColor = GUI.skin.label.normal.textColor
                },
                fontSize = fontSize,
                fontStyle = bold ? FontStyle.Bold : FontStyle.Normal
            };
            EditorGUI.LabelField(position, new GUIContent(text), headerStyle);
        }
        
        public static void Header(Rect position, GUIContent content, int fontSize = 15, TextAnchor alignment = TextAnchor.MiddleCenter, bool bold = true)
        {
            GUIStyle headerStyle = new GUIStyle()
            {
                alignment = alignment,
                normal = new GUIStyleState()
                {
                    textColor = GUI.skin.label.normal.textColor
                },
                fontSize = fontSize,
                fontStyle = bold ? FontStyle.Bold : FontStyle.Normal
            };
            EditorGUI.LabelField(position, content, headerStyle);
        }

        public static ParagraphRect ParaRect(string text, float spacing = 3.0f, float maxWidth = 0.0f)
        {
            return ParaRect(text, 0, 0, spacing, maxWidth);
        }
        
        public static ParagraphRect ParaRect(string text, int padLeft, int padRight, float spacing = 3.0f, float maxWidth = 0.0f)
        {
            GUIStyle tempStyle = new GUIStyle(paragraphStyle)
            {
                wordWrap = false
            };

            GUIContent content = new GUIContent(text);
            float width = tempStyle.CalcSize(content).x;
            if (width > maxWidth && maxWidth > 0.0f)
                width = maxWidth;
            float height = 0.0f;
            if (text.Contains("\n"))
            {
                string[] paras = text.Split("\n");
                content = new GUIContent(paras[0]);
                height += paragraphStyle.CalcHeight(content, width);
                for (int i = 1; i < paras.Length; i++)
                {
                    content = new GUIContent(paras[i]);
                    height += spacing + paragraphStyle.CalcHeight(content, width);
                }
            }
            else
            {
                height = paragraphStyle.CalcHeight(content, width);
            }
            return new ParagraphRect(0, 0, width, height, padLeft, padRight);
        }

        public static void Paragraphs(ParagraphRect paraRect, string text, float spacing = 3.0f)
        {
            Paragraphs(paraRect, text, TextAnchor.MiddleLeft, spacing);
        }
        
        public static void Paragraphs(ParagraphRect paraRect, string text, TextAnchor alignment = TextAnchor.MiddleLeft, float spacing = 3.0f)
        {
            GUIStyle tempStyle = new GUIStyle(paragraphStyle)
            {
                alignment = alignment
            };
            GUIContent content;
            Rect padded = paraRect.rectPadded;
            Rect[] rects = new Rect[2];
            if (text.Contains("\n"))
            {
                string[] paras = text.Split("\n");
                content = new GUIContent(paras[0]);
                rects[1] = Rect.zero;
                rects[0] = new Rect(padded);
                rects[0].height = paragraphStyle.CalcHeight(content, padded.width);
                GUI.Label(rects[0], content, tempStyle);
                for (int i = 1; i < paras.Length; i++)
                {
                    content = new GUIContent(paras[i]);
                    rects[1] = new Rect(rects[0]);
                    rects[0] = new Rect(padded);
                    rects[0].y = rects[1].y + rects[1].height + spacing;
                    rects[0].height = paragraphStyle.CalcHeight(content, padded.width);
                    GUI.Label(rects[0], content, tempStyle);
                }
            }
            else
            {
                content = new GUIContent(text);
                GUI.Label(padded, content, tempStyle);
            }
        }

        public static void SeparatorBar()
        {
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        }

        public static void GreyRect(float width, float height, bool expandWidth = false, bool expandHeight = false)
        {
            GUIStyle greyRectStyle = new GUIStyle(GUI.skin.horizontalSlider)
            {
                fixedWidth = 0,
                fixedHeight = 0,
                margin = new RectOffset(0, 0, 0, 0),
                padding = new RectOffset(0, 0, 0, 0),
                border = new RectOffset(0, 0, 0, 0),
                stretchHeight = true
            };
            GUILayout.Label("", greyRectStyle, GUILayout.Width(width), GUILayout.Height(height), GUILayout.ExpandWidth(expandWidth), GUILayout.ExpandHeight(expandHeight || height <= 0.0f));
        }
        
        public static void GreyRect(float minWidth, float maxWidth, float minHeight, float maxHeight)
        {
            GUIStyle greyRectStyle = new GUIStyle(GUI.skin.horizontalSlider)
            {
                fixedWidth = 0,
                fixedHeight = 0,
                margin = new RectOffset(0, 0, 0, 0),
                padding = new RectOffset(0, 0, 0, 0),
                border = new RectOffset(0, 0, 0, 0),
                stretchHeight = true
            };
            GUILayout.Label("", greyRectStyle, GUILayout.MinWidth(minWidth), GUILayout.MaxWidth(maxWidth), GUILayout.MinHeight(minHeight), GUILayout.MaxHeight(maxHeight));
        }

        /*public static void GreyRect(Rect posRect)
        {
            int controlID = GUIUtility.GetControlID(0, FocusType.Passive, posRect);
            posRect = EditorGUI.PrefixLabel(posRect, controlID, GUIContent.none, GUI.skin.horizontalSlider);
            if (Event.current.type == EventType.Repaint)
            {
                GUI.skin.horizontalSlider.Draw(posRect, GUIContent.none, controlID);
            }
        }*/

        /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

        public static Vector2Int Vector2Field_AdjustButtons(Rect position, Vector2Int vecIn, DualAxis axisLabels = DualAxis.XY)
        {
            Vector2Int vecOut = vecIn;

            float coordLabelWidth = GUI.skin.verticalScrollbarUpButton.fixedWidth + 1;
            float coordInputWidth = position.width >= 124 + 2 * coordLabelWidth ? 60 : (position.width >= 64 + 2 * coordLabelWidth ? (position.width - 4 - 2 * coordLabelWidth) / 2 : 30);
            Rect[] positions = new Rect[8]
            {
                new Rect(position)
                {
                    position = position.position,
                    width = coordLabelWidth
                },
                new Rect(position)
                {
                    position = position.position + new Vector2(0, -GUI.skin.verticalScrollbarUpButton.fixedHeight),
                    width = GUI.skin.verticalScrollbarUpButton.fixedWidth + coordInputWidth
                },
                new Rect(position)
                {
                    position = position.position + new Vector2(0, EditorGUIUtility.singleLineHeight),
                    width = GUI.skin.verticalScrollbarUpButton.fixedWidth + coordInputWidth
                },

                new Rect(position)
                {
                    position = position.position + new Vector2(coordLabelWidth, 0),
                    width = coordInputWidth
                },

                new Rect(position)
                {
                    position = position.position + new Vector2(coordLabelWidth + coordInputWidth + 4, 0),
                    width = coordLabelWidth
                },
                new Rect(position)
                {
                    position = position.position + new Vector2(coordLabelWidth + coordInputWidth + 4, -GUI.skin.verticalScrollbarUpButton.fixedHeight),
                    width = GUI.skin.verticalScrollbarUpButton.fixedWidth + coordInputWidth
                },
                new Rect(position)
                {
                    position = position.position + new Vector2(coordLabelWidth + coordInputWidth + 4, EditorGUIUtility.singleLineHeight),
                    width = GUI.skin.verticalScrollbarUpButton.fixedWidth + coordInputWidth
                },

                new Rect(position)
                {
                    position = position.position + new Vector2(coordLabelWidth * 2 + coordInputWidth + 4, 0),
                    width = coordInputWidth
                }
            };

            EditorGUI.LabelField(positions[0], (axisLabels == DualAxis.YZ ? "Y" : "X"), EditorStylesExtras.labelCentred);
            vecOut.x = EditorGUI.DelayedIntField(positions[3], vecOut.x);

            vecOut.x -= GUI.Button(positions[1], "", GUI.skin.verticalScrollbarUpButton) ? 0 : 1;
            vecOut.x += GUI.Button(positions[2], "", GUI.skin.verticalScrollbarDownButton) ? 0 : 1;

            EditorGUI.LabelField(positions[4], (axisLabels == DualAxis.XY ? "Y" : "Z"), EditorStylesExtras.labelCentred);
            vecOut.y = EditorGUI.DelayedIntField(positions[7], vecOut.y);

            vecOut.y -= GUI.Button(positions[5], "", GUI.skin.verticalScrollbarUpButton) ? 0 : 1;
            vecOut.y += GUI.Button(positions[6], "", GUI.skin.verticalScrollbarDownButton) ? 0 : 1;

            return vecOut;
        }
        
        public static Vector2Int[] Vector2RangeField_AdjustButtons(Rect position, Vector2Int vecMinIn, Vector2Int vecMaxIn, DualAxis axisLabels = DualAxis.XY)
        {
            Vector2Int vecMinOut = vecMinIn, vecMaxOut = vecMaxIn;

            float bWidth = GUI.skin.verticalScrollbarUpButton.fixedWidth;
            float[] widths = new float[]
            {
                bWidth + 1,
                position.width >= 4 * (60 + bWidth + 1) + 2 * 4 + 16 ? 60 :
                    (position.width < 4 * (30 + bWidth + 1) + 2 * 4 + 16 ? 30 :
                    (position.width - 2 * 4 - 16) / 4 - (bWidth + 1)),
                4,
                12
            };
            Rect[] positions = new Rect[17]
            {
                new Rect(position)
                {
                    position = position.position,
                    width = widths[0]
                },
                    new Rect(position)
                    {
                        position = position.position + new Vector2(0, -GUI.skin.verticalScrollbarUpButton.fixedHeight),
                        width = widths[0] + widths[1]
                    },
                    new Rect(position)
                    {
                        position = position.position + new Vector2(0, EditorGUIUtility.singleLineHeight),
                        width = widths[0] + widths[1]
                    },

                new Rect(position)
                {
                    position = position.position + new Vector2(widths[0], 0),
                    width = widths[1]
                },

                new Rect(position)
                {
                    position = position.position + new Vector2(widths[0] + widths[1] + widths[2], 0),
                    width = widths[0]
                },
                    new Rect(position)
                    {
                        position = position.position + new Vector2(widths[0] + widths[1] + widths[2], -GUI.skin.verticalScrollbarUpButton.fixedHeight),
                        width = widths[0] + widths[1]
                    },
                    new Rect(position)
                    {
                        position = position.position + new Vector2(widths[0] + widths[1] + widths[2], EditorGUIUtility.singleLineHeight),
                        width = widths[0] + widths[1]
                    },

                new Rect(position)
                {
                    position = position.position + new Vector2(2 * widths[0] + widths[1] + widths[2], 0),
                    width = widths[1]
                },

                // - - - - -

                new Rect(position)
                {
                    position = position.position + new Vector2(2 * widths[0] + 2 * widths[1] + widths[2], 0),
                    width = widths[3]
                },
                
                // - - - - -

                new Rect(position)
                {
                    position = position.position + new Vector2(2 * widths[0] + 2 * widths[1] + widths[2] + widths[3], 0),
                    width = widths[0]
                },
                    new Rect(position)
                    {
                        position = position.position + new Vector2(2 * widths[0] + 2 * widths[1] + widths[2] + widths[3], -GUI.skin.verticalScrollbarUpButton.fixedHeight),
                        width = widths[0] + widths[1]
                    },
                    new Rect(position)
                    {
                        position = position.position + new Vector2(2 * widths[0] + 2 * widths[1] + widths[2] + widths[3], EditorGUIUtility.singleLineHeight),
                        width = widths[0] + widths[1]
                    },

                new Rect(position)
                {
                    position = position.position + new Vector2(3 * widths[0] + 2 * widths[1] + widths[2] + widths[3], 0),
                    width = widths[1]
                },

                new Rect(position)
                {
                    position = position.position + new Vector2(3 * widths[0] + 3 * widths[1] + 2 * widths[2] + widths[3], 0),
                    width = widths[0]
                },
                    new Rect(position)
                    {
                        position = position.position + new Vector2(3 * widths[0] + 3 * widths[1] + 2 * widths[2] + widths[3], -GUI.skin.verticalScrollbarUpButton.fixedHeight),
                        width = widths[0] + widths[1]
                    },
                    new Rect(position)
                    {
                        position = position.position + new Vector2(3 * widths[0] + 3 * widths[1] + 2 * widths[2] + widths[3], EditorGUIUtility.singleLineHeight),
                        width = widths[0] + widths[1]
                    },

                new Rect(position)
                {
                    position = position.position + new Vector2(4 * widths[0] + 3 * widths[1] + 2 * widths[2] + widths[3], 0),
                    width = widths[1]
                }

            };

            EditorGUI.LabelField(positions[0], (axisLabels == DualAxis.YZ ? "Y" : "X"), EditorStylesExtras.labelCentred);
            vecMinOut.x = EditorGUI.DelayedIntField(positions[3], vecMinOut.x);

            vecMinOut.x -= GUI.Button(positions[1], "", GUI.skin.verticalScrollbarUpButton) ? 0 : 1;
            vecMinOut.x += GUI.Button(positions[2], "", GUI.skin.verticalScrollbarDownButton) ? 0 : 1;
            if (vecMinOut.x >= vecMaxOut.x)
                vecMaxOut.x = vecMinOut.x + 1;

            EditorGUI.LabelField(positions[4], (axisLabels == DualAxis.XY ? "Y" : "Z"), EditorStylesExtras.labelCentred);
            vecMinOut.y = EditorGUI.DelayedIntField(positions[7], vecMinOut.y);

            vecMinOut.y -= GUI.Button(positions[5], "", GUI.skin.verticalScrollbarUpButton) ? 0 : 1;
            vecMinOut.y += GUI.Button(positions[6], "", GUI.skin.verticalScrollbarDownButton) ? 0 : 1;
            if (vecMinOut.y >= vecMaxOut.y)
                vecMaxOut.y = vecMinOut.y + 1;

            EditorGUI.LabelField(positions[8], "–", EditorStylesExtras.labelCentred);

            EditorGUI.LabelField(positions[9], (axisLabels == DualAxis.YZ ? "Y" : "X"), EditorStylesExtras.labelCentred);
            vecMaxOut.x = EditorGUI.DelayedIntField(positions[12], vecMaxOut.x);

            vecMaxOut.x -= GUI.Button(positions[10], "", GUI.skin.verticalScrollbarUpButton) ? 0 : 1;
            vecMaxOut.x += GUI.Button(positions[11], "", GUI.skin.verticalScrollbarDownButton) ? 0 : 1;
            if (vecMaxOut.x <= vecMinOut.x)
                vecMinOut.x = vecMaxOut.x - 1;

            EditorGUI.LabelField(positions[13], (axisLabels == DualAxis.XY ? "Y" : "Z"), EditorStylesExtras.labelCentred);
            vecMaxOut.y = EditorGUI.DelayedIntField(positions[16], vecMaxOut.y);

            vecMaxOut.y -= GUI.Button(positions[14], "", GUI.skin.verticalScrollbarUpButton) ? 0 : 1;
            vecMaxOut.y += GUI.Button(positions[15], "", GUI.skin.verticalScrollbarDownButton) ? 0 : 1;
            if (vecMaxOut.y <= vecMinOut.y)
                vecMinOut.y = vecMaxOut.y - 1;

            return new Vector2Int[] { vecMinOut, vecMaxOut };
        }

        public static float Slider(Rect position, float value, float minValue, float maxValue, bool showNumberField = true)
        {
            if (showNumberField)
            {
                Rect numFieldRect = new Rect(position)
                {
                    width = 50,
                    x = position.x + position.width - 50
                };
                position.width -= 55;
                value = GUI.HorizontalSlider(position, value, minValue, maxValue);
                value = EditorGUI.DelayedFloatField(numFieldRect, value);
            }
            else
            {
                value = GUI.HorizontalSlider(position, value, minValue, maxValue);
            }
            return value;
        }
        
        public static float SteppedSlider(Rect position, float value, float minValue, float maxValue, int stepCount, bool showNumberField = true)
        {
            if (minValue > maxValue)
            {
                float ph = minValue;
                minValue = maxValue;
                maxValue = ph;
            }

            float range = maxValue - minValue;
            float stepSize = range / (float)stepCount;
            int step = Mathf.RoundToInt((value - minValue) / stepSize);

            return SteppedSlider(position, step, minValue, stepSize, stepCount, showNumberField);
        }

        public static float SteppedSlider(Rect position, int step, float minValue, float stepSize, int maxSteps, bool showNumberField = true)
        {
            if (step > maxSteps)
                step = maxSteps;
            if (step < 0)
                step = 0;

            float value = minValue + stepSize * (float)step;
            float maxValue = minValue + stepSize * (float)maxSteps;
            value = Slider(position, value, minValue, maxValue, showNumberField);
            step = Mathf.RoundToInt((value - minValue) / stepSize);
            return minValue + (float)step * stepSize;
        }

        public static int IntSlider(Rect position, int value, int minValue, int maxValue, bool showNumberField = true)
        {
            if (minValue > maxValue)
            {
                int ph = minValue;
                minValue = maxValue;
                maxValue = ph;
            }
            int maxSteps = maxValue - minValue;
            int step = value - minValue;

            if (showNumberField)
            {
                Rect numFieldRect = new Rect(position)
                {
                    width = 50,
                    x = position.x + position.width - 52
                };
                position.width = numFieldRect.width;
                value = (int)SteppedSlider(position, step, minValue, 1, maxSteps);
                value = EditorGUI.DelayedIntField(numFieldRect, value);
            }
            else
            {
                value = (int)SteppedSlider(position, step, minValue, 1, maxSteps);
            }
            return value;
        }

        public static float[] DualMinMaxSlider(Rect position, float minValue, float maxValue, float minLimit, float maxLimit, float minDifference = 0.0f, bool showNumberField = true)
        {
            if (minLimit > maxLimit)
            {
                float ph = minLimit;
                minLimit = maxLimit;
                maxLimit = ph;
            }
            if (minDifference < 0.0f)
                minDifference = 0.0f;

            float hTotalHalf = position.height / 2.0f;
            float hLine = EditorGUIUtility.singleLineHeight;

            Rect sliderPos = new Rect(position);
            sliderPos.height = hLine;
            sliderPos.y = position.y + hTotalHalf - hLine;

            minValue = Slider(sliderPos, minValue, minLimit, maxLimit, showNumberField);
            if (minValue > maxLimit - minDifference)
                minValue = maxLimit - minDifference;
            if (maxValue < minValue + minDifference)
                maxValue = minValue + minDifference;
            sliderPos.y += hLine;

            maxValue = Slider(sliderPos, maxValue, minLimit, maxLimit, showNumberField);
            if (maxValue < minLimit + minDifference)
                maxValue = minLimit + minDifference;
            if (minValue > maxValue - minDifference)
                minValue = maxValue - minDifference;

            return new float[2] { minValue, maxValue };
        }
        
        public static float[] DualMinMaxSteppedSlider(Rect position, float minValue, float maxValue, float minLimit, float maxLimit, int stepCount, int minStepDifference = 0, bool showNumberField = true)
        {
            if (minLimit > maxLimit)
            {
                float ph = minLimit;
                minLimit = maxLimit;
                maxLimit = ph;
            }
            if (minStepDifference < 0)
                minStepDifference = 0;
            float minFloatDiff = ((maxLimit - minLimit) / (float)stepCount) * (float)minStepDifference;

            float hTotalHalf = position.height / 2.0f;
            float hLine = EditorGUIUtility.singleLineHeight;

            Rect sliderPos = new Rect(position);
            sliderPos.height = hLine;
            sliderPos.y = position.y + hTotalHalf - hLine;

            minValue = SteppedSlider(sliderPos, minValue, minLimit, maxLimit, stepCount, showNumberField);
            if (minValue > maxLimit - minFloatDiff)
                minValue = maxLimit - minFloatDiff;
            if (maxValue < minValue + minFloatDiff)
                maxValue = minValue + minFloatDiff;
            sliderPos.y += hLine;

            maxValue = SteppedSlider(sliderPos, maxValue, minLimit, maxLimit, stepCount, showNumberField);
            if (maxValue < minLimit + minFloatDiff)
                maxValue = minLimit + minFloatDiff;
            if (minValue > maxValue - minFloatDiff)
                minValue = maxValue - minFloatDiff;

            return new float[2] { minValue, maxValue };
        }
        
        public static int[] DualMinMaxIntSlider(Rect position, int minValue, int maxValue, int minLimit, int maxLimit, int minDifference = 0)
        {
            float[] fVals = DualMinMaxSteppedSlider(position, minValue, maxValue, minLimit, maxLimit, minDifference);
            return new int[] { (int)fVals[0], (int)fVals[1] };
        }

        /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

        public static bool CursorInRect(Rect rect)
        {
            return rect.Contains(Event.current.mousePosition);
        }

        public static bool CursorInRects(Rect[] rects)
        {
            return rects.Contains(Event.current.mousePosition);
        }
    }

    public static class EditorExt_GUIStyle
    {
        public static Vector2 CalcWordWrappedSize(this GUIStyle style, GUIContent content)
        {
            Vector2 size = style.CalcSize(content);
            size = new Vector2(size.x, style.CalcHeight(content, size.x));
            return size;
        }
    }

    public static class EditorExt_GameObject
    {
        // Initially found :
        // https://answers.unity.com/questions/213140/programmatically-assign-an-editor-icon-to-a-game-o.html
        // Led to:
        // https://answers.unity.com/questions/542890/scene-color-object-marking.html
        // Which in turn led to:
        // https://github.com/Thundernerd/Unity3D-IconManager

        public static void SetIcon(this GameObject gameObject, string iconPath)
        {
            SetIcon(gameObject, (Texture2D)AssetDatabase.LoadAssetAtPath(iconPath, typeof(Texture2D)));
        }

        public static void SetIcon(this GameObject gameObject, Texture2D icon)
        {
#if UNITY_2021_2_OR_NEWER
            EditorGUIUtility.SetIconForObject(gameObject, icon);
#else
            if (setIconForObjectMethodInfo == null)
            {
                Type type = typeof(EditorGUIUtility);
                setIconForObjectMethodInfo =
                    type.GetMethod("SetIconForObject", BindingFlags.Static | BindingFlags.NonPublic);
            }

            setIconForObjectMethodInfo.Invoke(null, new object[] {obj, icon});
#endif
        }
    }

    public static class Ext_GenericMenu
    {
        public static void AddMenuItems(this GenericMenu menu, MenuReturn<int[]> returnFunc, DropdownMenuItem[] items, string path = null, int[] activeItem = null, int[] parentIndex = null)
        {
            int n = 0;
            int[] index = parentIndex == null ? new int[1] : new int[parentIndex.Length + 1];
            if (parentIndex != null)
                index.CopyFrom(parentIndex);
            for (int i = 0; i < items.Length; i++)
            {
                //index[index.Length - 1] = items[i].index < 0 ? n : items[i].index;
                index[index.Length - 1] = n;
                if (items[i].content == null)
                {
                    Debug.Log("Separator added");
                    menu.AddSeparator(path);
                }
                else
                {
                    if (items[i].submenu == null)
                    {
                        GUIContent itemContent = new GUIContent(items[i].content);
                        if (path != null)
                            itemContent.text = path + items[i].content.text;
                        if (items[i].enabled)
                        {
                            menu.AddItem(itemContent, index == activeItem, indVal => returnFunc.Invoke((int[])indVal), new int[index.Length].CopyFrom(index));
                        }
                        else
                            menu.AddDisabledItem(itemContent);
                    }
                    else
                    {
                        string newPath = (path == null ? "" : path) + items[i].content.text + "/";
                        menu.AddMenuItems(returnFunc, items[i].submenu, newPath, activeItem, index);
                    }
                    n++;
                }
            }
        }
        
        public static void SetMenuItems(this GenericMenu menu, MenuReturn<int[]> returnFunc, string[] items, int[] activeItem = null)
        {
            menu.Clear();
            DropdownMenuItem[] _items = new DropdownMenuItem[items.Length];
            for (int i = 0; i < items.Length; i++)
            {
                _items[i] = new DropdownMenuItem(items[i], i, true);
            }
            menu.AddMenuItems(returnFunc, _items, null, activeItem);
        }
                
        public static void SetMenuItems(this GenericMenu menu, MenuReturn<int[]> returnFunc, List<string> items, int[] activeItem = null)
        {
            menu.Clear();
            DropdownMenuItem[] _items = new DropdownMenuItem[items.Count];
            for (int i = 0; i < items.Count; i++)
            {
                _items[i] = new DropdownMenuItem(items[i], i, true);
            }
            menu.AddMenuItems(returnFunc, _items, null, activeItem);
        }
                
        public static void SetMenuItems(this GenericMenu menu, MenuReturn<int[]> returnFunc, GUIContent[] items, int[] activeItem = null)
        {
            menu.Clear();
            DropdownMenuItem[] _items = new DropdownMenuItem[items.Length];
            for (int i = 0; i < items.Length; i++)
            {
                _items[i] = new DropdownMenuItem(items[i], i, true);
            }
            menu.AddMenuItems(returnFunc, _items, null, activeItem);
        }
                
        public static void SetMenuItems(this GenericMenu menu, MenuReturn<int[]> returnFunc, List<GUIContent> items, int[] activeItem = null)
        {
            menu.Clear();
            DropdownMenuItem[] _items = new DropdownMenuItem[items.Count];
            for (int i = 0; i < items.Count; i++)
            {
                _items[i] = new DropdownMenuItem(items[i], i, true);
            }
            menu.AddMenuItems(returnFunc, _items, null, activeItem);
        }

        public static void SetMenuItems(this GenericMenu menu, MenuReturn<int[]> returnFunc, DropdownMenuItem[] items, int[] activeItem = null)
        {
            menu.Clear();
            menu.AddMenuItems(returnFunc, items, null, activeItem);
        }

        public static void SetMenuItems(this GenericMenu menu, MenuReturn<int[]> returnFunc, List<DropdownMenuItem> items, int[] activeItem = null)
        {
            menu.Clear();
            menu.AddMenuItems(returnFunc, items.ToArray(), null, activeItem);
        }

        public static void Clear(this GenericMenu menu, bool allowDuplicateNames = true)
        {
            menu = new GenericMenu() { allowDuplicateNames = allowDuplicateNames };
        }
    }
}

// CUSTOM EDITOR WINDOW CLASS TEMPLATE
/*public class PH_CLASS_NAME : EditorWindow
{
    protected static EditorWindow _Window = null;
    public static EditorWindow Window
    {
        get
        {
            if (_Window == null)
                _Window = GetWindow(typeof(PH_CLASS_NAME));
            return _Window;
        }
    }

    #region [ OBJECTS / COMPONENTS ]



    #endregion

    #region [ OBJECT-COMPONENT VALIDATION ]



    #endregion

    #region [ PROPERTIES ]

    private Vector2 scrollPos = new Vector2();

    #region < REGION TOGGLES >

    bool showGuide = false;

    bool showObjectFields = true;

    private bool showChunkSel = true;
    private bool showChunkSelType = true;
    private int chunkSelType = 0;
    private string[] chunkSelOptions = new string[] { "Single Chunk", "Chunk Range" };

    private bool showOptions = true;

    #endregion

    #region < UTILITY OBJECTS / DATA >

    GUIContent label = new GUIContent();
    Rect elementRect;

    #endregion

    private float lastAvWidth;

    #endregion

    / * - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - * /

    #region [ BUILT-IN UNITY FUNCTIONS ]

    void OnGUI()
    {
        GUI.enabled = true;

        float slHeight = EditorGUIUtility.singleLineHeight;
        bool darkTheme = GUI.skin.label.normal.textColor.ApproximatelyEquals(new Color(0.824f, 0.824f, 0.824f, 1.000f), 0.005f);
        RectOffset rOffZero = new RectOffset(0, 0, 0, 0);

        EditorGUILayout.BeginHorizontal(EditorStyles.inspectorFullWidthMargins);
        {
            float avWidth = EditorGUILayout.BeginVertical(EditorStylesExtras.noMarginsNoPadding).width;
            if (avWidth > 0.0f)
                lastAvWidth = avWidth;
            else
                avWidth = lastAvWidth;
            {
 
            }
            EditorGUILayout.EndVertical();
        }
        EditorGUILayout.EndHorizontal();
    }

    void OnValidate()
    {
        // <Cache loading & w/e here>
    }

    [MenuItem("Window/<Submenu Path>/<Window Name>")]
    public static void ShowWindow()
    {
        _Window = GetWindow(typeof(PH_CLASS_NAME));
        Window.titleContent = new GUIContent("<Window Title>");
    }

    #endregion

    / * - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - * /

}*/
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

namespace RizeLibrary.LayerRatio
{
	public class LayerRatioEditor : EditorWindow
	{
		private const int BorderSize = 10;
		private const float KeyWidth = 10;
		private const float KeyHeight = 20;
		private int _selectedKeyIndex;
		private float _newRatio;
		private bool _mouseDrag;
		private bool _needsRepaint;
		private Rect _gradientPreviewRect;
		private Rect[] _keyRects;
		private LayerRatio _layerRatio;
		private Event _guiEvent;

		private float NewRatio
		{
			get => _newRatio;
			set => _newRatio = Mathf.Clamp01(value);
		}

		private void OnGUI()
	    {
		    _guiEvent = Event.current;
		    
	        Draw();
	        HandleInput();
	        
	        if (_needsRepaint)
	        {
	            _needsRepaint = false;
	            Repaint();
	        }
	    }

	    private void Draw()
	    {
			_gradientPreviewRect = new Rect(BorderSize, BorderSize, position.width - BorderSize * 2, 50);
			
			// グラデーションの描画
			GUI.DrawTexture(_gradientPreviewRect, _layerRatio.GetTexture((int)_gradientPreviewRect.width));

			// キーの描画
			_keyRects = new Rect[_layerRatio.NumberKeys];
			for (int i = 0; i < _layerRatio.NumberKeys; i++)
			{
				LayerRatio.Key key = _layerRatio.GetKey(i);
				Rect keyRect = new Rect(_gradientPreviewRect.x + _gradientPreviewRect.width * key.Ratio - KeyWidth / 2f, _gradientPreviewRect.yMax + BorderSize, KeyWidth, KeyHeight);
				if (i == _selectedKeyIndex)
				{
					DrawPentagon(new Rect(keyRect.x - 2, keyRect.y - 2, keyRect.width + 4, keyRect.height + 4), Color.white);
				}
				DrawPentagon(keyRect, key.Color);
				_keyRects[i] = keyRect;
			}

	        var settingsRect = new Rect(BorderSize, _keyRects[0].yMax + BorderSize, position.width - BorderSize * 2, position.height);
	        GUILayout.BeginArea(settingsRect);
	        EditorGUI.BeginChangeCheck();
	        
	        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
	        
			GUILayout.BeginHorizontal(GUILayout.Width(_gradientPreviewRect.width - 40));
			{
			    EditorGUILayout.Space(10);

			    // キーの色を変更
			    EditorGUILayout.LabelField("Color", GUILayout.Width(50));
			    Color newColour = EditorGUILayout.ColorField(_layerRatio.GetKey(_selectedKeyIndex).Color, GUILayout.Width(100));

			    EditorGUILayout.Space(10);

			    // キーの割合
			    if (!Mathf.Approximately(NewRatio, _layerRatio.GetKey(_selectedKeyIndex).Ratio))
			    {
			        NewRatio = _layerRatio.GetKey(_selectedKeyIndex).Ratio;
			    }
			    NewRatio = FloatField("Ratio", NewRatio, GUILayout.Width(50));
			    
			    EditorGUILayout.LabelField("/ 1", GUILayout.Width(20));

			    // 変更を検知したら更新する
			    if (EditorGUI.EndChangeCheck())
			    {
			        _layerRatio.UpdateKeyColor(_selectedKeyIndex, newColour);
			        
			        _selectedKeyIndex = _layerRatio.UpdateKeyTime(_selectedKeyIndex, NewRatio);
			        _needsRepaint = true;
			    }
			}
			GUILayout.EndHorizontal();
			
			EditorGUILayout.Space(20);
			
	        // リセット
	        if (GUILayout.Button("Reset", GUILayout.Height(20)))
	        {
		        _layerRatio.Reset();
		        _selectedKeyIndex = 0;
		        _needsRepaint = true;
	        }
	        
	        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
	        
	        // キーの色をランダムにするかどうか
	        _layerRatio.RandomizeColour = EditorGUILayout.Toggle("Random Color", _layerRatio.RandomizeColour);
	        
	        GUILayout.EndArea();
	    }
	    
	    /// <summary>
	    /// 五角形のキーを描画
	    /// </summary>
	    /// <param name="rect">矩形</param>
	    /// <param name="color">色</param>
	    private void DrawPentagon(Rect rect, Color color)
	    {
		    Handles.color = color;
		    
		    // 三角形の頂点
		    var point1 = new Vector3(rect.x, rect.y + rect.height / 3);
		    var point2 = new Vector3(rect.x + rect.width, rect.y + rect.height / 3);
		    var point3 = new Vector3(rect.x + rect.width / 2, rect.y);
		    
		    // 三角形を描画
		    Handles.DrawAAConvexPolygon(point1, point2, point3);
		    
		    // 四角形の頂点
		    // point1 -= new Vector3(0, 1);
		    // point2 -= new Vector3(0, 1);
		    var point4 = new Vector3(rect.x, rect.y + rect.height);
		    var point5 = new Vector3(rect.x + rect.width, rect.y + rect.height);
		    
		    // 四角形を描画
		    Handles.DrawAAConvexPolygon(point2, point1, point4, point5);
	    }
	    
		private float FloatField(string label, float value, params GUILayoutOption[] options)
	    {
	        EditorGUILayout.BeginHorizontal();
	        {
	            const float valueSpeed = 3f;
	            string labelName = label + GUIUtility.GetControlID(FocusType.Passive);
	            string floatName = labelName + "_float_field";
	 
	            GUI.SetNextControlName(labelName);
	            EditorGUILayout.LabelField(label, options);
	            int id = GUIUtility.GetControlID(FocusType.Passive);
	            Rect lastRect = GUILayoutUtility.GetLastRect();
	 
	            GUI.SetNextControlName(floatName);
	            value = EditorGUILayout.FloatField(value);
	 
	            bool rectFlag = lastRect.Contains(_guiEvent.mousePosition);
	 
	            if (_guiEvent.button == 0)
	            {
	                switch (_guiEvent.type)
	                {
	                    case EventType.MouseDown:
	                        if (rectFlag)
	                        {
	                            GUIUtility.hotControl = id;
	                            GUI.FocusControl(labelName);
	                            _guiEvent.Use();
	                        }
	                        break;
	                    case EventType.MouseDrag:
	                        if (GUIUtility.hotControl == id)
	                        {
	                            float dis = _guiEvent.delta.x;
	                            value = value * 100.0f + dis * valueSpeed;
	                            value = Mathf.Floor(Mathf.Abs(value)) / 100f * Mathf.Sign(value);
	                            GUI.FocusControl(labelName);
	                            _selectedKeyIndex = _layerRatio.UpdateKeyTime(_selectedKeyIndex, value);
	                            _needsRepaint = true;
	                        }
	                        break;
	                    case EventType.MouseUp:
	                        if (GUIUtility.hotControl == id)
	                        {
	                            GUIUtility.hotControl = 0;
	                        }
	                        break;
	                }
	            }
	 
	            EditorGUIUtility.AddCursorRect(lastRect, MouseCursor.SlideArrow);
	        }
	        EditorGUILayout.EndHorizontal();
	        
	        return value;
	    }

	    /// <summary>
	    /// 入力処理
	    /// </summary>
	    public void HandleInput()
	    {
			SelectedKey();

			if (_guiEvent.type == EventType.MouseUp && _guiEvent.button == 0)
			{
				_mouseDrag = false;
			}
			
			UpdateKey();

			DeleteKey();
			
			// Escapeキーで閉じる
			if (_guiEvent.type == EventType.KeyDown && _guiEvent.keyCode == KeyCode.Escape)
			{
				Close();
			}
	    }

	    /// <summary>
	    /// キーの選択 / 追加
	    /// </summary>
	    /// <param name="guiEvent"></param>
	    private void SelectedKey()
	    {
		    if (_guiEvent.mousePosition.y > _keyRects[0].yMax + KeyHeight) { return; }
		    
		    // マウスの左クリック
		    if (_guiEvent.type == EventType.MouseDown && _guiEvent.button == 0)
		    {
			    // キーがあれば選択
			    for (int i = 0; i < _keyRects.Length; i++)
			    {
				    if (_keyRects[i].Contains(_guiEvent.mousePosition))
				    {
					    _mouseDrag = true;
					    _selectedKeyIndex = i;
					    _needsRepaint = true;
					    NewRatio = _layerRatio.GetKey(_selectedKeyIndex).Ratio;
					    break;
				    }
			    }

			    // キーが選択されていない場合は新しいキーを追加
			    if (!_mouseDrag)
			    {
				    float newTime = Mathf.InverseLerp(_gradientPreviewRect.x, _gradientPreviewRect.xMax, _guiEvent.mousePosition.x);
				    Color interpolatedColour = _layerRatio.Evaluate(newTime);
				    Color randomColour = new Color(Random.value, Random.value, Random.value);

				    NewRatio = Mathf.Round(newTime * 100) / 100;
				    _selectedKeyIndex = _layerRatio.AddKey(_layerRatio.RandomizeColour ? randomColour : interpolatedColour, newTime);
				    _mouseDrag = true;
				    _needsRepaint = true;
			    }
		    }
	    }

	    /// <summary>
	    /// キーの更新
	    /// </summary>
	    /// <param name="guiEvent"></param>
	    private void UpdateKey()
	    {
		    // マウスの左ドラッグ
		    if (_mouseDrag && _guiEvent.type == EventType.MouseDrag && _guiEvent.button == 0)
		    {
			    float keyTime = Mathf.InverseLerp(_gradientPreviewRect.x, _gradientPreviewRect.xMax, _guiEvent.mousePosition.x);
			    NewRatio = Mathf.Round(keyTime * 100) / 100;
			    _selectedKeyIndex = _layerRatio.UpdateKeyTime(_selectedKeyIndex, keyTime);
			    _needsRepaint = true;
		    }
	    }

	    /// <summary>
	    /// キーの削除
	    /// </summary>
	    /// <param name="guiEvent"></param>
	    private void DeleteKey()
	    {
		    // マウスの右クリック/Deleteキー/Backspaceキーのいずれかでキーを削除
		    if (_guiEvent.type == EventType.KeyDown && _guiEvent.keyCode is KeyCode.Delete or KeyCode.Backspace || _guiEvent.type == EventType.MouseDown && _guiEvent.button == 1)
		    {
			    _layerRatio.RemoveKey(_selectedKeyIndex);
			    if (_selectedKeyIndex >= _layerRatio.NumberKeys)
			    {
				    _selectedKeyIndex--;
			    }
			    _needsRepaint = true;
		    }
	    }

	    /// <summary>
	    /// レイヤー比率を設定
	    /// </summary>
	    /// <param name="layerRatio">対象クラス</param>
	    public void SetLayerRatio(LayerRatio layerRatio)
	    {
	        _layerRatio = layerRatio;
	    }

	    private void OnEnable()
	    {
	        titleContent.text = "LayerRatio Editor";
	        position.Set(position.x, position.y, 340, 340);
	        minSize = new Vector2(340, 150);
	        maxSize = new Vector2(340, 1080);
	    }

	    private void OnDisable()
	    {
	        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
	    }
	}
}

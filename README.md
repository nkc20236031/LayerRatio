# LayerRatio
Unityで使えるGradient風エディタ拡張
<br>
基本的にはInspector上で編集してEvaluateやGetTextureあたりを使う
<br><br>
![image](https://github.com/user-attachments/assets/36b54d1a-33e6-4fea-96d7-8b2fee5db306)

### 変数の宣言
```cs
public LayerRatio _layerRatio;
[SerializeField] private LayerRatio _layerRatio;
```

## 提供されているメソッド
### Evaluate: 比率に応じた色の取得
```cs
public Color Evaluate(float ratio);
```

### AddKey: キーを追加
```cs
public int AddKey(Color color, float ratio);
```

### RemoveKey: キーを削除
```cs
public void RemoveKey(int index);
```

### UpdateKeyColor: キーの色を更新
```cs
public void UpdateKeyColor(int index, Color color);
```

### GetKey: キーを取得
```cs
public Key GetKey(int index);
```

### GetTexture: テクスチャを取得
```cs
public Texture2D GetTexture(int width);
```

### Reset: 初期化
```cs
public void Reset();
```

using UnityEngine;

public class CoinController : MonoBehaviour
{
    public float coinSpeedZ = 0.01f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //現在のオブジェクトの位置を基準にして移動
        transform.Translate(0, 0, coinSpeedZ);

        //画面外に出たらオブジェクト破棄
        if (transform.position.z > 42.0f)
        {
            Destroy(gameObject);
        }
    }
}

using UnityEngine;

public class UnitController : MonoBehaviour
{
    //移動設定
    float moveForce = 10.0f;
    float jumpForce = 5.0f;

    Vector3 targetPosition;

    Animator anim;

    //ジャンプ
    bool isJump, isJumpWait;
    float jumpWaitTimer;

    GameObject director;



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        anim = GetComponent<Animator>();
        director = GameObject.Find("GameDirector");
        targetPosition = transform.position;

    }



    void Update()
    {
        ////移動(WASDで移動)※挙動がおかしかったので別の方法で移動
        //float h = -Input.GetAxis("Horizontal"); // A/D または 矢印左右
        //float v = -Input.GetAxis("Vertical");   // W/S または 矢印前後

        //// 移動方向のベクトルを作成
        //Vector3 moveDir = new Vector3(h, 0, v).normalized;

        //// 入力がある時だけ移動と回転の処理をする
        //if (moveDir.magnitude > 0.1f)
        //{
        //    // 現在の垂直方向（Y軸）の速度を保ちつつ、水平方向の速度を上書き
        //    GetComponent<Rigidbody>().linearVelocity = new Vector3(
        //        moveDir.x * moveForce,
        //        GetComponent<Rigidbody>().linearVelocity.y,
        //        moveDir.z * moveForce
        //    );

        //    // キャラクターを入力方向に向かせる
        //    transform.rotation = Quaternion.LookRotation(moveDir);
        //}
        //else
        //{
        //    // キーを離した時は、水平方向の速度を止める（滑り防止）
        //    GetComponent<Rigidbody>().linearVelocity = new Vector3(
        //        0,
        //        GetComponent<Rigidbody>().linearVelocity.y,
        //        0
        //    );
        //}

        ////移動(マウスクリックしたポイントに移動)
        //if (Input.GetMouseButtonUp(0))
        //{
        //    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        //    GameObject tgt = null;

        //    foreach(RaycastHit hit in Physics.RaycastAll(ray))
        //    {
        //        tgt = (hit.transform.name.Equals("Cube")) ? hit.transform.gameObject : null;
        //        if (tgt != null) break;
        //    }

        //    if(tgt != null)
        //    {
        //        targetPosition = tgt.transform.position;
        //        transform.forward = targetPosition;
        //    }
        //}

        if (Input.GetMouseButtonUp(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            // RaycastAllよりシンプルなRaycastの方が制御しやすいです
            if (Physics.Raycast(ray, out hit))
            {
                if (hit.transform.name.Contains("Cube"))
                {
                    targetPosition = hit.transform.position;

                    // 正しい向きの変え方
                    Vector3 lookDir = targetPosition - transform.position;
                    lookDir.y = 0; // 上下を見ないようにする
                    if (lookDir != Vector3.zero)
                    {
                        transform.rotation = Quaternion.LookRotation(lookDir);
                    }
                }
            }
        }

        //ジャンプ
        if (Input.GetKeyDown(KeyCode.Space)) 
        {
            if (!isJump && !isJumpWait)//ジャンプしていないかつジャンプ待機していない場合
            {
                anim.Play("Jump", 0, 0);//jumpというアニメーションを最初（0秒）から再生

                isJumpWait = true;
                jumpWaitTimer = 0.2f;


            }
        }

        if (isJumpWait)
        {
            jumpWaitTimer -= Time.deltaTime;
            if (jumpWaitTimer < 0)
            {
                GetComponent<Rigidbody>().linearVelocity = transform.up * jumpForce;

                isJumpWait = false;
                isJump = true;
            }
        }


        // 2. 移動の実行（目標地点が今の場所と違う時だけ動く）
        if (Vector3.Distance(transform.position, targetPosition) > 0.1f)
        {
            Vector3 nextPos = Vector3.MoveTowards(transform.position, targetPosition, moveForce * Time.deltaTime);
            nextPos.y = transform.position.y; // 地面にめり込まないように
            transform.position = nextPos;
        }

        ////移動実行
        //targetPosition.y = transform.position.y;//y座標は変えない
        //transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveForce * Time.deltaTime);

        ////ひっくり返らないようにする
        //transform.eulerAngles = new Vector3(0, transform.eulerAngles.y, 0);
    }

    private void OnCollisionEnter(Collision collision)
    {
        isJump = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        string name = other.gameObject.name;

        if (!name.Contains("Coin")) return;

        Destroy(other.gameObject);

        if (name.Contains("Gold"))
        {
            director.GetComponent<GameDirector>().AddScore(100);
        }else if (name.Contains("Silver"))
        {
            director.GetComponent<GameDirector>().AddScore(50);
        }
        else if (name.Contains("Wood"))
        {
            director.GetComponent<GameDirector>().AddScore(10);
        }
    }
}

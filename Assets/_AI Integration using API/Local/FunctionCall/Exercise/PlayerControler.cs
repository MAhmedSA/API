using UnityEngine;

using System.Collections;

public class PlayerControler : MonoBehaviour
{

    [SerializeField] GameObject bullet;
    [SerializeField] Transform bulletPosition;
   public static  PlayerControler controler;
    float velocity = 0.5f;

    private void Start()
    {
        controler=this;
    }

    public GameObject Attack()
    {
        GameObject bullet_ = Instantiate(bullet, bulletPosition.position, Quaternion.identity);
        return bullet;
    }
    public Vector3 MovePlayer(int dir) {
       return this.gameObject.transform.position += new Vector3(10, 0, 0) * dir;
    }
    

}

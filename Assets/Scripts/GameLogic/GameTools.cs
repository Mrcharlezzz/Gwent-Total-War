using UnityEngine;


public class GameTools{
    public static GameObject cardPrefab;
    
    public static GameObject CreateCardInObject(Card card, GameObject obj, int owner){
        GameObject body = MonoBehaviour.Instantiate(cardPrefab,new Vector3(0,0,0),Quaternion.identity);
        body.transform.SetParent(obj.transform,false);
        Carddisplay display = body.GetComponent<Carddisplay>();
        display.displayId = card.id;
        display.update = true;
        card.owner=owner;
        return body;
    }
}
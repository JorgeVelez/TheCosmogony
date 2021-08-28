using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RaindropFX;

namespace RaindropFX {
    public class CachePool_HDRP : MonoBehaviour {

        public int counter = 0;

        List<Raindrop_HDRP> raindrops = new List<Raindrop_HDRP>();

        public void Init() {
            counter = 0;
            raindrops.Clear();
        }

        public void Recycle(Raindrop_HDRP raindrop) {
            raindrops.Add(raindrop);
            counter = raindrops.Count;
        }

        public Raindrop_HDRP GetRaindrop() {
            if (counter > 0) {
                Raindrop_HDRP temp = raindrops[0];
                raindrops.RemoveAt(0);
                counter = raindrops.Count;
                return temp;
            } else return new Raindrop_HDRP();
        }

    }
}

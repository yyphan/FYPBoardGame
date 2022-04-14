using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BoardGame
{
    public class RoundTimer
    {
        private int totalRounds;
        private int remainingRounds;

        public RoundTimer(int totalRounds)
        {
            Reset(totalRounds);
        }

        public void Reset()
        {
            this.remainingRounds = this.totalRounds;
        }

        public void Reset(int totalRounds)
        {
            this.totalRounds = totalRounds;
            Reset();
        }

        public void UpdateTimer()
        {
            this.remainingRounds--;
        }

        public int GetRemainingRounds()
        {
            return this.remainingRounds;
        }

        public bool HasElapsed()
        {
            return this.remainingRounds <= 0;
        }

        public void EndTimer()
        {
            this.remainingRounds = 0;
        }
    }

}
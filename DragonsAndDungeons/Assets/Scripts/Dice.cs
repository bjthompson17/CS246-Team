using System;

static class Dice {
  static Random rand = new Random();
  public static (int[] Nums, int Result) Roll(int count, int sides, AdvantageType adv = AdvantageType.None) {
    int[] results = new int[count];
    int sum = 0;
    for(int i = 0;i < count;i++) {
      int num = rand.Next(1,sides + 1);

      if(adv != AdvantageType.None) {
        int num2 = rand.Next(1,sides + 1);
        if(adv == AdvantageType.Advantage && num2 > num)
          num = num2;
        else if(adv == AdvantageType.Disadvantage && num2 < num)
          num = num2;
      }

      results[i] = num;
      sum += results[i];
    }
    return (results, sum);
  }
}
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/*
             * 若表1.1仅包含1、4两个示例
             * 即{青绿、蜷缩、浊响} = 好瓜
             * {乌黑、稍蜷、沉闷} = 坏瓜
             * 那么，每个属性存在两个可能的值，加上一个泛化，是三种假设
             * 加上最后一个空集，则假设空间为3*3*3+1=28种可能性
             * 但是在28种假设当中，正例可以筛掉大部分
             * 不过，版本空间除了通过搜索完整的假设空间来获取外，还可以对正例进行最大泛化
             * 那么得到的泛化种类有
             * 1-青绿、蜷缩、浊响
             * 2-青绿、蜷缩、*
             * 3-青绿、*、浊响
             * 4-*、蜷缩、浊响
             * 5-青绿、*、*
             * 6-*、蜷缩、*
             * 7-*、*、浊响
             * 8-*、*、*
             * 不过，第八个假设包含了唯一存在的反例，所以不计，则版本空间为1~7
             * 
             */

namespace ConsoleApplication1
{
    class Program
    {
        /*
         * 若表1.1包含4个示例，第一个属性有两个可能值，后面每个属性存在3个可能值
         * 最后加上一个空集，则假设空间为3*4*4+1=49种假设
         * 若不计沉余，即忽略掉合取式互相包容互相等价的情况，则49种假设中可以任取1~49个表达式作为析合范式
         * 则析合范式的组合有2^49种
         * 若考虑沉余，则暂时忽略空集，因为对于前48种假设而言，任意组合事实上被【非空集合】这一析合范式包含
         * 所以大可以求出前48种假设的可能数之后，再计算。因为每一组合加上空集之后，都会多出一个新的组合
         * 最后单独计入空集作为一种组合即可。
         * 对于前48种假设
         * 无泛化的假设有：2*3*3=18种
         * 一种泛化的假设有：2*3+3*3+2*3=21种
         * 两种泛化的假设有：2+3+3=8种
         * 三种均泛化的假设有：1种
         * 若k=1时，48种假设任取一种即可，加上空集，则当k=1时，最多有49种假设
         * 若k=19时，无泛化的假设全部选满，加上空集，则当k=19时，最多有1种假设
         * 由于只有18种假设，所以可以令一个int32的末尾18位作为标记位，当某位为0时，则代表涵盖了该位所代表的无泛化假设，若为1，则恰好相反。
         */
        private static int[,] hypothesis = new int[48, 3]{
                {0,0,0},
                //代表{*，*，*}，范围0
                {0,0,1},
                {0,0,2},
                {0,0,3},
                {0,1,0},
                {0,2,0},
                {0,3,0},
                {1,0,0},
                {2,0,0},
                //代表{青绿，*，*}这样的二泛化假设，范围1~8
                {0,1,1},{0,1,2},{0,1,3},
                {0,2,1},{0,2,2},{0,2,3},
                {0,3,1},{0,3,2},{0,3,3},
                {1,0,1},{1,0,2},{1,0,3},
                {2,0,1},{2,0,2},{2,0,3},
                {1,1,0},{1,2,0},{1,3,0},
                {2,1,0},{2,2,0},{2,3,0},
                //代表{青绿，蜷缩，*}这样的一泛化假设，范围，9~29
                {1,1,1},{1,1,2},{1,1,3},
                {1,2,1},{1,2,2},{1,2,3},
                {1,3,1},{1,3,2},{1,3,3},
                {2,1,1},{2,1,2},{2,1,3},
                {2,2,1},{2,2,2},{2,2,3},
                {2,3,1},{2,3,2},{2,3,3},
                //代表无泛化的假设，30~47
            };
        private static int[] hypoInt;
        private static int[] noNullResult;
        //有了手动输入的假设空间还不够，我们需要把这些数组形状的假设转换成为int值
        private static void hypo2int()
        {
            StringBuilder sb = new StringBuilder();
            hypoInt = new int[48];
            //将无泛化的假设转换为int值备用
            for (int i = 30; i < 48; i++)
            {
                sb.Clear();
                for (int j = 0; j < 32; j++)
                {
                    if (j == i - 30 + 14)
                        sb.Append('0');
                    else
                        sb.Append('1');
                }
                hypoInt[i] = Convert.ToInt32(sb.ToString(), 2);
            }
            //将泛化假设的数组跟无泛化假设的数组比对，生成新的数组
            for (int i = 0; i < 30; i++)
            {
                sb.Clear();
                //填充前14位
                for (int j = 0; j < 14; j++)
                    sb.Append('1');
                //开始比对
                for (int j = 30; j < 48; j++)
                {
                    //判断泛化假设是否包含无泛化假设
                    if ((hypothesis[i, 0] != 0 && hypothesis[i, 0] - hypothesis[j, 0] != 0) ||
                       (hypothesis[i, 1] != 0 && hypothesis[i, 1] - hypothesis[j, 1] != 0) ||
                       (hypothesis[i, 2] != 0 && hypothesis[i, 2] - hypothesis[j, 2] != 0))
                        sb.Append('1');
                    else
                        sb.Append('0');
                }
                hypoInt[i] = Convert.ToInt32(sb.ToString(), 2);
            }
        }

        /// <summary>
        /// 给定一个k值，计算出非空假设的最大可能数
        /// </summary>
        /// <param name="k"></param>
        /// <returns></returns>
        static int GetNoNullResultByK(int k)
        {
            Stack<KeyValuePair<int, int>> stack = new Stack<KeyValuePair<int, int>>();
            int targetLength = k;
            //目前拼凑出来的析取范式
            int curHypo;
            //从前往后使用的偏移值
            int offset;
            //累计的假设数量
            int count = 0;
            //从前往后，依次使用
            for (int i = 0; i < 48; i++)
            {
                //初始化堆栈
                stack.Clear();
                stack.Push(new KeyValuePair<int, int>(hypoInt[i], i));
                curHypo = Convert.ToInt32("11111111111111111111111111111111", 2) & hypoInt[i];
                offset = i + 1;
                //开始循环
                while (true)
                {
                    //如果stack已经被弹空了，说明所有的都已经遍历了一次
                    if (stack.Count == 0)
                    {
                        break;
                    }
                    //如果达到目标高度，则弹出最上面的键值对继续尝试
                    if (stack.Count == targetLength)
                    {
                        offset = stack.Pop().Value + 1;
                        count++;
                        continue;
                    }
                    //如果偏移值到头了，则弹出两对继续尝试
                    if (offset >= 48)
                    {
                        if (stack.Count == 1)
                            break;
                        stack.Pop();
                        offset = stack.Pop().Value + 1;
                        continue;
                    }
                    //要是之前都没有终端，则判断可否往里加塞
                    //判断标准：
                    //假设现有数为001010，则可以加塞的数为000000、000010、001000
                    //即，第二个数至少要把一个1变为0，且不允许把0变成1
                    bool canAdd = true;
                    for (int j = 0; j < 18; j++)
                    {
                        //如果现在析取范式某位为0（已被泛化），且尝试加塞一个特例
                        if (isZero(j, curHypo) && !isZero(j, hypoInt[offset]))
                        {
                            canAdd = false;
                            continue;
                        }
                    }
                    if (canAdd)
                    {
                        stack.Push(new KeyValuePair<int, int>(hypoInt[offset], offset));
                        curHypo = Convert.ToInt32("11111111111111111111111111111111", 2);
                        foreach (KeyValuePair<int, int> kv in stack)
                        {
                            curHypo = curHypo & kv.Key;
                        }
                    }
                    offset++;
                }
            }
            Print(k, count);
            return count;
        }

        static bool isZero(int offset, int bit)
        {
            int a = bit;
            a = offset >> bit;
            return ((a & 1) == 0);
        }

        static void Print(int k, int count)
        {
            Console.WriteLine(string.Format("有{0}个合取式的析合范式最多有{1}个假设", k, count));
        }

        static void Main(string[] args)
        {
            //准备好整数化后的示例数组
            hypo2int();
            //计算1~48项的非空假设组合
            noNullResult = new int[18];
            for (int i = 1; i <= 18; i++)
            {
                noNullResult[i - 1] = GetNoNullResultByK(i);
            }
            //加上空集后输出
            for (int k = 1; k <= 19; k++)
            {
                if (k == 1)
                    Print(k, 49);
                else if (k == 19)
                    Print(k, 1);
                else
                {
                    //每个k值有加上空集和无空集两种情况，以及一个纯空集
                    Print(k, noNullResult[k - 1] * 2 + 1);
                }
            }
            Console.Read();
        }
    }
}
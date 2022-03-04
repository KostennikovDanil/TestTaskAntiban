using System;
using System.Collections.Generic;
using System.Linq;

namespace Antiban
{
    public class Antiban
    {
        private List<EventMessage> Messages = new List<EventMessage>();
        /// <summary>
        /// Добавление сообщений в систему, для обработки порядка сообщений
        /// </summary>
        /// <param name="eventMessage"></param>
        /// 

        private DateTime CheckBySeconds(double diff, AntibanResult preResult, int checkBy)
        {
            if (diff < checkBy && diff >= 0)
            {
                var res = preResult.SentDateTime.AddSeconds(checkBy - diff);
                return res;
            }

            return preResult.SentDateTime;
        }

        public void PushEventMessage(EventMessage eventMessage)
        {
            Messages.Add(eventMessage);
        }

        /// <summary>
        /// Вовзращает порядок отправок сообщений
        /// </summary>
        /// <returns></returns>
        public List<AntibanResult> GetResult()
        {
            var result = new List<AntibanResult>();
            result.Add(new AntibanResult { EventMessageId = Messages[0].Id, SentDateTime = Messages[0].DateTime });
            for(int i = 1; i < Messages.Count; i++)
            {
                var preResult = new AntibanResult { EventMessageId = Messages[i].Id, SentDateTime = Messages[i].DateTime };

                for(int j = 0; j < result.Count; j++)
                {
                    var resultMessage = Messages.FirstOrDefault(_ => _.Id == result[j].EventMessageId);
                    if (resultMessage == null)
                        throw new Exception("no AntibanResult in result list");

                    var diff = preResult.SentDateTime.Subtract(result[j].SentDateTime).TotalSeconds;

                    if (Messages[i].Phone == resultMessage.Phone)
                    {

                        if (Messages[i].Priority == resultMessage.Priority && resultMessage.Priority == 1)
                        {
                            if (diff < 0)
                                diff = Math.Abs(diff);

                            preResult.SentDateTime = CheckBySeconds(diff, preResult, 86400);
                        }
                        else
                            preResult.SentDateTime = CheckBySeconds(diff, preResult, 60);

                    }
                    if (Messages[i].Phone != Messages[j].Phone)
                        preResult.SentDateTime = CheckBySeconds(diff, preResult, 10);

                    if (j == result.Count - 1)
                    {
                        result.Add(preResult);
                        break;
                    }


                }
               
            }

            var resultSorter = result.OrderBy(_ => _.SentDateTime).ToList();
            
            return resultSorter;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Practical_work_10._5
{
    internal class MessageLog
    {
        public string Time { get; set; }

        public long Id { get; set; }

        public string Msg { get; set; }

        public string FirstName { get; set; }

        public int MessageId { get; set; }

        public MessageLog(string time, string msg, string firstName, long id, int messageId)
        {
            Time = time;
            Msg = msg;
            FirstName = firstName;
            Id = id;
            MessageId = messageId;
        }

    }
}

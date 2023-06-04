using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Empire.Models
{
    public interface IFeedbacker
    {
        public Task GiveFeedback();
    }

    public  class FeedbackTasks
    {
        private Queue<Task> tasks;
        private IFeedbacker feedbacker;
        
        public FeedbackTasks(IFeedbacker f)
        {
            tasks = new Queue<Task>();
            feedbacker = f;
        }

        //this method could aslo be named AddSingle
        public async System.Threading.Tasks.Task Add( IUnit unit, Tasks enumTask, List<Loc>? locs = null)
        {
            tasks.Enqueue (new Task(unit, enumTask, locs));
            await feedbacker.GiveFeedback();

        }
        public FeedbackTasks.Task Dequeue()
        {
            return tasks.Dequeue();
        }

        public bool IsEmpty()
        {
            return tasks.Count == 0;
        }

        public class Task
        {
            public Tasks kind;
            public IUnit unit;
            public List<Loc> locs;
            public Task(IUnit u, Tasks enumKind, List<Loc>? l)
            {
                kind = enumKind;
                unit = u;
                locs = l;
            }
        }
    }


    public enum Tasks
    {
        DelayAfterMove,
        DelayBeforeMove,
        DelayInbetweenSteps,
        StopBeingActive,

    }




}

(()=>{"use strict";addEventListener("message",({data:i})=>{const e={},a=i;if(!a.measures)return;const s=a.measures;e.labels=s.map(t=>t.utcTimeStamp.toLocaleString("it-IT",{year:void 0,month:void 0,day:void 0,hour:"2-digit",minute:"2-digit",second:"2-digit"})),e.lastValue=s[s.length-1].formattedValue,e.values=s.map(t=>t.value),e.tOnMilliseconds=a.timeSlotSize*e.values.reduce((t,n)=>t+n,0),e.graphId=a.graphId,postMessage(e)})})();
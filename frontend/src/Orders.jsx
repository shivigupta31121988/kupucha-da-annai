import React, { useEffect, useState } from 'react'
import axios from 'axios'

export default function Orders({onClose}){
  const [orders, setOrders] = useState([])
  const [loading, setLoading] = useState(false)

  const fetchOrders = async () => {
    setLoading(true)
    try{
      const res = await axios.get('/api/orders')
      setOrders(res.data)
    }catch(e){
      console.error(e)
    }finally{setLoading(false)}
  }

  useEffect(()=>{ fetchOrders() }, [])

  return (
    <div className="orders-panel">
      <div className="orders-header">
        <h2>Orders</h2>
        <div>
          <button onClick={fetchOrders}>Refresh</button>
          <button onClick={onClose} style={{marginLeft:8}}>Close</button>
        </div>
      </div>
      {loading ? <div>Loading...</div> : (
        <div className="orders-list">
          {orders.length === 0 && <div>No orders</div>}
          {orders.map(o => (
            <div key={o.id} className="order-row">
              <div className="col symbol">{o.symbol}</div>
              <div className="col qty">x{o.quantity}</div>
              <div className="col price">{o.price ?? '-'}</div>
              <div className="col side">{o.side}</div>
              <div className="col status">{o.status}</div>
              <div className="col time">{o.executedAt ? new Date(o.executedAt).toLocaleString() : new Date(o.createdAt).toLocaleString()}</div>
              <div style={{marginLeft:8}}>
                {o.status === 'scheduled' && <button onClick={()=>{ axios.post(`/api/orders/${o.id}/cancel`).then(fetchOrders).catch(()=>alert('Cancel failed')) }}>Cancel</button>}
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  )
}

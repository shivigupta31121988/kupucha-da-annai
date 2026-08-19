import React, { useEffect, useState } from 'react'
import axios from 'axios'

export default function Orders({onClose}){
  const [orders, setOrders] = useState([])
  const [loading, setLoading] = useState(false)
  const [orderToCancel, setOrderToCancel] = useState(null)
  const [cancelling, setCancelling] = useState(false)

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

  const confirmCancel = (order) => {
    setOrderToCancel(order)
  }

  const doCancel = async () => {
    if (!orderToCancel) return
    setCancelling(true)
    try{
      await axios.post(`/api/orders/${orderToCancel.id}/cancel`)
      await fetchOrders()
      setOrderToCancel(null)
    }catch(e){
      alert('Cancel failed')
    }finally{setCancelling(false)}
  }

  const closeModal = () => setOrderToCancel(null)

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
                {o.status === 'scheduled' && <button onClick={()=>confirmCancel(o)}>Cancel</button>}
              </div>
            </div>
          ))}
        </div>
      )}

      {orderToCancel && (
        <div className="modal-backdrop">
          <div className="modal">
            <div style={{marginBottom:12}}>
              <strong>Cancel Order</strong>
              <div style={{fontSize:13, color:'#444'}}>Are you sure you want to cancel order <strong>{orderToCancel.symbol}</strong> x{orderToCancel.quantity}?</div>
            </div>
            <div className="modal-actions">
              <button onClick={closeModal} disabled={cancelling}>No</button>
              <button onClick={doCancel} disabled={cancelling} style={{marginLeft:8}}>{cancelling ? 'Cancelling...' : 'Yes, cancel'}</button>
            </div>
          </div>
        </div>
      )}
    </div>
  )
}

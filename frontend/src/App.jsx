import React, { useEffect, useState } from 'react'
import axios from 'axios'

function Tile({stock}){
  return (
    <div className="tile">
      <div className="symbol">{stock.symbol}</div>
      <div className="name">{stock.name}</div>
      <div className="rates">
        <div>Market: {stock.marketRate}</div>
        <div>Yesterday: {stock.yesterday}</div>
        <div>Predicted: {stock.predicted}</div>
      </div>
      <button className="buy">Buy</button>
    </div>
  )
}

export default function App(){
  const [stocks, setStocks] = useState([])

  const refresh = () => {
    axios.get('/api/stocks').then(r=> setStocks(r.data)).catch(()=>{})
  }

  useEffect(()=>{
    refresh();
  },[])

  const createStock = async () => {
    const symbol = prompt('Symbol (e.g. KPU)')?.toUpperCase();
    if (!symbol) return;
    const name = prompt('Name for ' + symbol) ?? '';
    const market = parseFloat(prompt('Market rate') ?? '0');
    const yesterday = parseFloat(prompt('Yesterday price') ?? '0');
    const predicted = parseFloat(prompt('Predicted price') ?? '0');
    await axios.post('/api/stocks', { symbol, name, marketRate: market, yesterday, predicted });
    refresh();
  }

  const editStock = async (s) => {
    const name = prompt('Edit name', s.name) ?? s.name;
    const market = parseFloat(prompt('Market rate', s.marketRate) ?? s.marketRate);
    await axios.put('/api/stocks/' + s.symbol, { name, marketRate: market });
    refresh();
  }

  const placeOrder = async (s) => {
    const qty = parseInt(prompt('Quantity to buy') ?? '0');
    const price = parseFloat(prompt('Limit price (optional)') ?? '0');
    try{
      const res = await axios.post('/api/orders', { symbol: s.symbol, quantity: qty, price: price || null });
      alert('Order placed: ' + (res.data.status ?? 'scheduled'))
    }catch(e){
      alert('Order error')
    }
  }

  return (
    <div className="app">
      <h1>kupucha</h1>
      <div style={{display:'flex',justifyContent:'space-between',alignItems:'center',marginBottom:12}}>
        <div/>
        <div>
          <button onClick={createStock} style={{marginRight:8}}>Create Stock</button>
          <button onClick={refresh}>Refresh</button>
        </div>
      </div>
      <div className="board">
        {stocks.map(s => <div key={s.symbol} onDoubleClick={()=>editStock(s)}><Tile stock={s} /><div style={{height:8}}/><button onClick={()=>placeOrder(s)}>Buy</button></div>)}
      </div>
    </div>
  )
}

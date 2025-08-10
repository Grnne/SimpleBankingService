Салют, это резюме по заданию 3 на стажировке, сервис «Банковские счета»

init branch
the NodaTime plugin









EXPLAIN ANALYZE
SELECT a.id,
       a.balance,
       a.closed_at,
       a.created_at,
       a.credit_limit,
       a.currency,
       a.interest_rate,
       a.last_interest_accrual_at,
       a.owner_id,
       a.type,
       t0.id,
       t0.account_id,
       t0.amount,
       t0.counterparty_account_id,
       t0.currency,
       t0.description,
       t0.timestamp,
       t0.type
FROM accounts AS a
LEFT JOIN (
    SELECT t.id,
           t.account_id,
           t.amount,
           t.counterparty_account_id,
           t.currency,
           t.description,
           t.timestamp,
           t.type
    FROM transactions AS t
    WHERE t.timestamp <= '2026-04-10T00:00:00Z'
) AS t0 ON a.id = t0.account_id
WHERE a.owner_id = '11111111-1111-1111-1111-111111111111'
  AND a.created_at <= '2026-04-10T00:00:00Z'
ORDER BY a.id;

QUERY PLAN                                                                                                                                                       
Sort  (cost=17.54..17.55 rows=1 width=752) (actual time=0.032..0.033 rows=3 loops=1)                                                                             
  Sort Key: a.id                                                                                                                                                 
  Sort Method: quicksort  Memory: 25kB                                                                                                                           
  ->  Nested Loop Left Join  (cost=4.16..17.53 rows=1 width=752) (actual time=0.023..0.026 rows=3 loops=1)                                                       
        ->  Bitmap Heap Scan on accounts a  (cost=4.02..9.36 rows=1 width=140) (actual time=0.013..0.014 rows=2 loops=1)                                         
              Recheck Cond: (owner_id = '11111111-1111-1111-1111-111111111111'::uuid)                                                                            
              Filter: (created_at <= '2026-04-10 07:00:00+07'::timestamp with time zone)                                                                         
              Heap Blocks: exact=1                                                                                                                               
              ->  Bitmap Index Scan on ix_accounts_owner_id  (cost=0.00..4.01 rows=2 width=0) (actual time=0.007..0.007 rows=2 loops=1)                          
                    Index Cond: (owner_id = '11111111-1111-1111-1111-111111111111'::uuid)                                                                        
        ->  Index Scan using ix_transactions_account_id_timestamp on transactions t  (cost=0.14..8.16 rows=1 width=612) (actual time=0.004..0.005 rows=2 loops=2)
              Index Cond: ((account_id = a.id) AND ("timestamp" <= '2026-04-10 07:00:00+07'::timestamp with time zone))                                          
Planning Time: 0.143 ms                                                                                                                                          
Execution Time: 0.090 ms                                                                                                                                         

 